using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MagicEastern.ADOExt.IQueryable
{
    /// <summary>
    /// Translates a LINQ chain made of Where / OrderBy / OrderByDescending /
    /// ThenBy / ThenByDescending / Skip / Take into a SQL Server SELECT statement.
    /// Instances keep per-call state and are not thread-safe (use one instance per thread).
    /// </summary>
    public class SqlExpressionVisitor : ExpressionVisitor
    {
        private static int _sequence;

        private static readonly HashSet<Type> ParameterizedTypes = new HashSet<Type>
        {
            typeof(string), typeof(char), typeof(DateTime), typeof(DateTimeOffset), typeof(byte[]),
            typeof(float), typeof(double), typeof(decimal),
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong)
        };

        // ---- per-call state (reset in GetSqlQuery) ----
        private int _seq;
        private string _paraSymbol = "@";
        private List<Parameter> _parameters;
        private Func<string, string> _getColumnName;

        private readonly StringBuilder _where = new StringBuilder();
        private readonly StringBuilder _scratch = new StringBuilder();
        // Each OrderBy starts a new group placed in front of the previous ones
        // (LINQ semantics: the last OrderBy is the primary key); ThenBy appends to the first group.
        private readonly List<List<string>> _orderGroups = new List<List<string>>();
        private int? _skip;
        private int? _take;

        // Nodes (by reference) that reference a lambda parameter; computed once per lambda.
        private readonly HashSet<Expression> _dependent = new HashSet<Expression>();
        private readonly Dictionary<Expression, object> _evalCache = new Dictionary<Expression, object>();
        private readonly DependencyAnalyzer _analyzer;

        // Non-null only while a lambda body is being translated.
        private StringBuilder _sb;

        public SqlExpressionVisitor()
        {
            _analyzer = new DependencyAnalyzer(_dependent);
        }

        /// <param name="node">The LINQ expression tree.</param>
        /// <param name="basequery">Base select query that has neither WHERE nor ORDER BY.</param>
        /// <param name="columnNameFunc">Maps a property name to a column name.</param>
        /// <param name="parameterSymbol">SQL Server uses "@"; Oracle uses ":".</param>
        public Sql GetSqlQuery(Expression node, Sql basequery, Func<string, string> columnNameFunc = null, string parameterSymbol = "@")
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (basequery == null) throw new ArgumentNullException(nameof(basequery));

            _where.Clear();
            _orderGroups.Clear();
            _dependent.Clear();
            _evalCache.Clear();
            _skip = null;
            _take = null;
            _sb = null;

            _seq = Interlocked.Increment(ref _sequence);
            _paraSymbol = parameterSymbol;
            _parameters = new List<Parameter>();
            _getColumnName = columnNameFunc ?? (n => "[" + n.Replace("]", "]]") + "]");

            Visit(node);

            var sql = new StringBuilder(basequery.Text, basequery.Text.Length + _where.Length + 128);

            if (_where.Length > 0)
            {
                sql.Append(" WHERE ").Append(_where);
            }

            if (_orderGroups.Count > 0)
            {
                sql.Append(" ORDER BY ");
                bool first = true;
                foreach (List<string> group in _orderGroups)
                {
                    foreach (string item in group)
                    {
                        if (!first) sql.Append(", ");
                        sql.Append(item);
                        first = false;
                    }
                }
            }

            // FETCH is only valid after OFFSET in SQL Server.
            if (_skip.HasValue || _take.HasValue)
            {
                sql.Append(" OFFSET ")
                   .Append((_skip ?? 0).ToString(CultureInfo.InvariantCulture))
                   .Append(" ROWS");
            }

            if (_take.HasValue)
            {
                sql.Append(" FETCH NEXT ")
                   .Append(_take.Value.ToString(CultureInfo.InvariantCulture))
                   .Append(" ROWS ONLY");
            }

            return new Sql(sql.ToString(), _parameters);
        }

        #region Entry point: fold parameter-independent subtrees

        /// <summary>
        /// While translating a lambda body, any subtree that doesn't reference the lambda
        /// parameter (closure variables, DateTime.Now, local lists...) is evaluated once
        /// and emitted as a SQL value.
        /// </summary>
        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            if (_sb != null && !_dependent.Contains(node))
            {
                _sb.Append(FormatValue(Evaluate(node)));
                return node;
            }

            return base.Visit(node);
        }

        #endregion

        #region Queryable chain

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(System.Linq.Queryable))
            {
                return VisitQueryableCall(node);
            }

            if (_sb == null)
            {
                throw new NotSupportedException(
                    "Method '" + node.Method.Name + "' is not supported outside of a predicate.");
            }

            return VisitScalarMethodCall(node);
        }

        private Expression VisitQueryableCall(MethodCallExpression node)
        {
            // Visit the source first so operators are recorded in call order.
            Visit(node.Arguments[0]);

            switch (node.Method.Name)
            {
                case "Where":
                    if (_where.Length > 0) _where.Append(" AND ");
                    _where.Append('(');
                    Translate(GetLambda(node.Arguments[1]), _where, true);
                    _where.Append(')');
                    break;

                case "OrderBy":
                    _orderGroups.Insert(0, new List<string> { TranslateOrder(node, " ASC") });
                    break;

                case "OrderByDescending":
                    _orderGroups.Insert(0, new List<string> { TranslateOrder(node, " DESC") });
                    break;

                case "ThenBy":
                    GetCurrentOrderGroup().Add(TranslateOrder(node, " ASC"));
                    break;

                case "ThenByDescending":
                    GetCurrentOrderGroup().Add(TranslateOrder(node, " DESC"));
                    break;

                case "Skip":
                    _skip = Convert.ToInt32(Evaluate(node.Arguments[1]), CultureInfo.InvariantCulture);
                    break;

                case "Take":
                    _take = Convert.ToInt32(Evaluate(node.Arguments[1]), CultureInfo.InvariantCulture);
                    break;

                default:
                    throw new NotSupportedException("Queryable." + node.Method.Name + " is not supported.");
            }

            return node;
        }

        private List<string> GetCurrentOrderGroup()
        {
            if (_orderGroups.Count == 0)
            {
                throw new InvalidOperationException("ThenBy must follow an OrderBy.");
            }

            return _orderGroups[0];
        }

        private static LambdaExpression GetLambda(Expression argument)
        {
            while (argument.NodeType == ExpressionType.Quote)
            {
                argument = ((UnaryExpression)argument).Operand;
            }

            var lambda = argument as LambdaExpression;
            if (lambda == null || lambda.Parameters.Count != 1)
            {
                throw new NotSupportedException("Only single-parameter lambdas are supported (indexed overloads are not).");
            }

            return lambda;
        }

        private string TranslateOrder(MethodCallExpression node, string direction)
        {
            _scratch.Clear();
            Translate(GetLambda(node.Arguments[1]), _scratch, false);
            return _scratch.Append(direction).ToString();
        }

        /// <summary>Writes the lambda body straight into <paramref name="target"/> (no intermediate builders).</summary>
        private void Translate(LambdaExpression lambda, StringBuilder target, bool asCondition)
        {
            _analyzer.Analyze(lambda.Body); // one pass instead of one walk per node
            _sb = target;
            try
            {
                if (asCondition)
                {
                    VisitCondition(lambda.Body);
                }
                else
                {
                    Visit(lambda.Body);
                }
            }
            finally
            {
                _sb = null;
            }
        }

        #endregion

        #region Predicate / value translation

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    WriteLogical(node, node.NodeType == ExpressionType.AndAlso ? " AND " : " OR ");
                    return node;

                case ExpressionType.And:
                case ExpressionType.Or:
                    if (node.Type == typeof(bool) || node.Type == typeof(bool?))
                    {
                        WriteLogical(node, node.NodeType == ExpressionType.And ? " AND " : " OR ");
                        return node;
                    }
                    break;

                case ExpressionType.Coalesce:
                    _sb.Append("ISNULL(");
                    Visit(node.Left);
                    _sb.Append(", ");
                    Visit(node.Right);
                    _sb.Append(')');
                    return node;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    // "= NULL" never matches in SQL, so comparisons with null become IS [NOT] NULL.
                    string nullTest = node.NodeType == ExpressionType.Equal ? " IS NULL" : " IS NOT NULL";
                    if (IsNullValue(node.Right))
                    {
                        _sb.Append('(');
                        Visit(node.Left);
                        _sb.Append(nullTest).Append(')');
                        return node;
                    }
                    if (IsNullValue(node.Left))
                    {
                        _sb.Append('(');
                        Visit(node.Right);
                        _sb.Append(nullTest).Append(')');
                        return node;
                    }
                    break;
            }

            string op = GetBinaryOperator(node.NodeType);

            _sb.Append('(');
            Visit(node.Left);
            _sb.Append(' ').Append(op).Append(' ');
            Visit(node.Right);
            _sb.Append(')');
            return node;
        }

        private static string GetBinaryOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal: return "=";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked: return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked: return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked: return "*";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.And: return "&";
                case ExpressionType.Or: return "|";
                case ExpressionType.ExclusiveOr: return "^";
                default:
                    throw new NotSupportedException("Binary operator '" + type + "' is not supported.");
            }
        }

        private void WriteLogical(BinaryExpression node, string sqlOperator)
        {
            _sb.Append('(');
            VisitCondition(node.Left);
            _sb.Append(sqlOperator);
            VisitCondition(node.Right);
            _sb.Append(')');
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    if (node.Type == typeof(bool) || node.Type == typeof(bool?))
                    {
                        _sb.Append("(NOT ");
                        VisitCondition(node.Operand);
                    }
                    else
                    {
                        _sb.Append("(~");
                        Visit(node.Operand);
                    }
                    _sb.Append(')');
                    return node;

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    _sb.Append("(-");
                    Visit(node.Operand);
                    _sb.Append(')');
                    return node;

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.TypeAs:
                case ExpressionType.Quote:
                    // Conversions (e.g. int -> int?, enum -> int) are transparent in SQL.
                    Visit(node.Operand);
                    return node;

                default:
                    throw new NotSupportedException("Unary operator '" + node.NodeType + "' is not supported.");
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Type declaringType = node.Member.DeclaringType;

            if (declaringType != null && Nullable.GetUnderlyingType(declaringType) != null)
            {
                if (node.Member.Name == "HasValue")
                {
                    _sb.Append('(');
                    Visit(node.Expression);
                    _sb.Append(" IS NOT NULL)");
                    return node;
                }

                if (node.Member.Name == "Value")
                {
                    Visit(node.Expression);
                    return node;
                }
            }

            if (declaringType == typeof(string) && node.Member.Name == "Length")
            {
                _sb.Append("LEN(");
                Visit(node.Expression);
                _sb.Append(')');
                return node;
            }

            if (StripConvert(node.Expression) is ParameterExpression)
            {
                _sb.Append(_getColumnName(node.Member.Name));
                return node;
            }

            throw new NotSupportedException(
                "Member access '" + node + "' is not supported (only direct properties of the row type).");
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            _sb.Append("(CASE WHEN ");
            VisitCondition(node.Test);
            _sb.Append(" THEN ");
            Visit(node.IfTrue);
            _sb.Append(" ELSE ");
            Visit(node.IfFalse);
            _sb.Append(" END)");
            return node;
        }

        private Expression VisitScalarMethodCall(MethodCallExpression node)
        {
            MethodInfo method = node.Method;

            if (IsLikeMethod(method))
            {
                WriteLike(node);
                return node;
            }

            if (IsInMethod(method))
            {
                WriteIn(node);
                return node;
            }

            if (method.DeclaringType == typeof(string) && node.Object != null)
            {
                switch (method.Name)
                {
                    case "ToUpper":
                    case "ToUpperInvariant":
                        return WriteFunction("UPPER(", node.Object, ")", node);
                    case "ToLower":
                    case "ToLowerInvariant":
                        return WriteFunction("LOWER(", node.Object, ")", node);
                    case "Trim":
                        if (method.GetParameters().Length == 0)
                        {
                            return WriteFunction("LTRIM(RTRIM(", node.Object, "))", node);
                        }
                        break;
                }
            }

            throw new NotSupportedException("Method '" + method.Name + "' is not supported.");
        }

        private Expression WriteFunction(string prefix, Expression operand, string suffix, Expression node)
        {
            _sb.Append(prefix);
            Visit(operand);
            _sb.Append(suffix);
            return node;
        }

        private void WriteLike(MethodCallExpression node)
        {
            string name = node.Method.Name;
            string prefix = name == "StartsWith" ? string.Empty : "%";
            string suffix = name == "EndsWith" ? string.Empty : "%";
            Expression argument = node.Arguments[0];

            _sb.Append('(');
            Visit(node.Object);
            _sb.Append(" LIKE ");

            if (!_dependent.Contains(argument))
            {
                var value = Evaluate(argument) as string;
                if (value == null)
                {
                    throw new NotSupportedException("A null pattern is not supported in " + name + ".");
                }

                _sb.Append(FormatValue(prefix + EscapeLike(value) + suffix));
            }
            else
            {
                // Column-to-column pattern: concatenate the wildcards (no escaping possible).
                if (prefix.Length > 0) _sb.Append('\'').Append(prefix).Append("' + ");
                Visit(argument);
                if (suffix.Length > 0) _sb.Append(" + '").Append(suffix).Append('\'');
            }

            _sb.Append(')');
        }

        private void WriteIn(MethodCallExpression node)
        {
            // Either list.Contains(item) (instance) or Enumerable.Contains(list, item) (static).
            Expression collection = node.Object ?? node.Arguments[0];
            Expression item = node.Object != null ? node.Arguments[0] : node.Arguments[1];

            if (_dependent.Contains(collection))
            {
                throw new NotSupportedException("Contains on a row member is not supported.");
            }

            var values = Evaluate(collection) as IEnumerable;
            if (values == null)
            {
                throw new NotSupportedException("Contains requires a non-null collection.");
            }

            IEnumerator enumerator = values.GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                {
                    _sb.Append("(1 = 0)");
                    return;
                }

                _sb.Append('(');
                Visit(item);
                _sb.Append(" IN (");

                do
                {
                    _sb.Append(FormatValue(enumerator.Current));
                    if (!enumerator.MoveNext()) break;
                    _sb.Append(", ");
                }
                while (true);

                _sb.Append("))");
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// Writes an expression in a boolean (search-condition) position. SQL Server has no
        /// boolean value type, so a bare bool column or value needs "= 1".
        /// </summary>
        private void VisitCondition(Expression node)
        {
            Visit(node);
            if (!IsPredicate(node))
            {
                _sb.Append(" = 1");
            }
        }

        private bool IsPredicate(Expression node)
        {
            if (!_dependent.Contains(node))
            {
                return false; // evaluated to a 1/0 value
            }

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return true;

                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.Not:
                    return node.Type == typeof(bool) || node.Type == typeof(bool?);

                case ExpressionType.Call:
                    MethodInfo method = ((MethodCallExpression)node).Method;
                    return IsLikeMethod(method) || IsInMethod(method);

                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)node;
                    return member.Member.Name == "HasValue"
                           && member.Member.DeclaringType != null
                           && Nullable.GetUnderlyingType(member.Member.DeclaringType) != null;

                default:
                    return false;
            }
        }

        #endregion

        #region Unsupported node types (fail loudly instead of emitting broken SQL)

        protected override Expression VisitNew(NewExpression node) { throw Unsupported(node); }
        protected override Expression VisitNewArray(NewArrayExpression node) { throw Unsupported(node); }
        protected override Expression VisitInvocation(InvocationExpression node) { throw Unsupported(node); }
        protected override Expression VisitMemberInit(MemberInitExpression node) { throw Unsupported(node); }
        protected override Expression VisitListInit(ListInitExpression node) { throw Unsupported(node); }
        protected override Expression VisitTypeBinary(TypeBinaryExpression node) { throw Unsupported(node); }
        protected override Expression VisitIndex(IndexExpression node) { throw Unsupported(node); }
        protected override Expression VisitParameter(ParameterExpression node) { throw Unsupported(node); }
        protected override Expression VisitLambda<T>(Expression<T> node) { throw Unsupported(node); }

        private static NotSupportedException Unsupported(Expression node)
        {
            return new NotSupportedException(
                "Expression '" + node + "' (" + node.NodeType + ") is not supported.");
        }

        #endregion

        #region Helpers

        private static bool IsLikeMethod(MethodInfo method)
        {
            if (method.DeclaringType != typeof(string)) return false;

            string name = method.Name;
            if (name != "Contains" && name != "StartsWith" && name != "EndsWith") return false;

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
        }

        private static bool IsInMethod(MethodInfo method)
        {
            return method.Name == "Contains" && method.DeclaringType != typeof(string);
        }

        private static Expression StripConvert(Expression node)
        {
            while (node != null
                   && (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked))
            {
                node = ((UnaryExpression)node).Operand;
            }

            return node;
        }

        private bool IsNullValue(Expression node)
        {
            node = StripConvert(node);

            var constant = node as ConstantExpression;
            if (constant != null)
            {
                return constant.Value == null;
            }

            return !_dependent.Contains(node) && Evaluate(node) == null;
        }

        /// <summary>Evaluates a parameter-independent expression; results are cached per call.</summary>
        private object Evaluate(Expression node)
        {
            object value;
            if (_evalCache.TryGetValue(node, out value))
            {
                return value;
            }

            if (!TryEvaluateFast(node, out value))
            {
                // Slow path: compiling a delegate costs far more than the reflection above.
                value = Expression.Lambda<Func<object>>(Expression.Convert(node, typeof(object))).Compile()();
            }

            _evalCache[node] = value;
            return value;
        }

        /// <summary>Handles constants, closure fields and trivial conversions without compiling.</summary>
        private static bool TryEvaluateFast(Expression node, out object value)
        {
            value = null;

            switch (node.NodeType)
            {
                case ExpressionType.Constant:
                    value = ((ConstantExpression)node).Value;
                    return true;

                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)node;
                    var field = member.Member as FieldInfo;
                    if (field == null)
                    {
                        return false;
                    }

                    if (field.IsStatic)
                    {
                        value = field.GetValue(null);
                        return true;
                    }

                    object target;
                    if (member.Expression == null || !TryEvaluateFast(member.Expression, out target) || target == null)
                    {
                        return false; // let the compiled path raise the proper NullReferenceException
                    }

                    value = field.GetValue(target);
                    return true;

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var unary = (UnaryExpression)node;
                    object operand;
                    if (unary.Method != null || !TryEvaluateFast(unary.Operand, out operand))
                    {
                        return false;
                    }

                    // Only conversions that leave the boxed value unchanged (e.g. int -> int?).
                    Type target2 = Nullable.GetUnderlyingType(unary.Type) ?? unary.Type;
                    if (operand == null || target2 == typeof(object) || target2 == operand.GetType())
                    {
                        value = operand;
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        private static string EscapeLike(string value)
        {
            if (value.IndexOfAny(new[] { '[', '%', '_' }) < 0)
            {
                return value;
            }

            // Bracket-escape LIKE wildcards; '[' must be replaced first.
            return value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        private string AddParameter(object value)
        {
            string name = "para_" + _seq.ToString(CultureInfo.InvariantCulture)
                          + "_" + _parameters.Count.ToString(CultureInfo.InvariantCulture);
            _parameters.Add(new Parameter(name, value));
            return _paraSymbol + name;
        }

        /// <summary>Returns SQL text for a value: NULL / 1 / 0 / GUID inline, everything else as a bound parameter.</summary>
        private string FormatValue(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            Type type = value.GetType();

            if (type == typeof(bool))
            {
                return (bool)value ? "1" : "0";
            }

            if (type == typeof(Guid))
            {
                return "'" + ((Guid)value).ToString("D") + "'";
            }

            if (type.IsEnum)
            {
                return FormatValue(Convert.ChangeType(value, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture));
            }

            if (ParameterizedTypes.Contains(type))
            {
                return AddParameter(value);
            }

            throw new NotSupportedException("Constant of type '" + type.FullName + "' is not supported.");
        }

        /// <summary>
        /// Single bottom-up pass that records every node containing a lambda parameter.
        /// Replaces the per-node ParameterFinder walk (which made translation O(n²)).
        /// </summary>
        private sealed class DependencyAnalyzer : ExpressionVisitor
        {
            private readonly HashSet<Expression> _result;
            private bool _found;

            public DependencyAnalyzer(HashSet<Expression> result)
            {
                _result = result;
            }

            public void Analyze(Expression body)
            {
                _found = false;
                Visit(body);
            }

            public override Expression Visit(Expression node)
            {
                if (node == null)
                {
                    return null;
                }

                bool outer = _found;
                _found = false;

                base.Visit(node);

                if (_found)
                {
                    _result.Add(node);
                }

                _found |= outer;
                return node;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                _found = true;
                return node;
            }
        }

        #endregion
    }
}