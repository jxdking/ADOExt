using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MagicEastern.ADOExt.IQueryable
{
    /// <summary>
    /// Translates a LINQ chain made of Where / OrderBy / OrderByDescending /
    /// ThenBy / ThenByDescending into a SQL Server SELECT statement.
    /// Instances keep per-call state and are not thread-safe.
    /// </summary>
    public class SqlExpressionVisitor : ExpressionVisitor
    {
        private static int SqlExpressionSeq = 0;
        public List<Parameter> Parameters = new List<Parameter>();


        private const string TableName = "sometable";

        private static readonly Dictionary<ExpressionType, string> BinaryOperators =
            new Dictionary<ExpressionType, string>
            {
                { ExpressionType.Equal, "=" },
                { ExpressionType.NotEqual, "<>" },
                { ExpressionType.LessThan, "<" },
                { ExpressionType.LessThanOrEqual, "<=" },
                { ExpressionType.GreaterThan, ">" },
                { ExpressionType.GreaterThanOrEqual, ">=" },
                { ExpressionType.Add, "+" },
                { ExpressionType.AddChecked, "+" },
                { ExpressionType.Subtract, "-" },
                { ExpressionType.SubtractChecked, "-" },
                { ExpressionType.Multiply, "*" },
                { ExpressionType.MultiplyChecked, "*" },
                { ExpressionType.Divide, "/" },
                { ExpressionType.Modulo, "%" },
                { ExpressionType.And, "&" },
                { ExpressionType.Or, "|" },
                { ExpressionType.ExclusiveOr, "^" }
            };

        // Collected state for the statement being built.
        private readonly List<string> _whereClauses = new List<string>();
        // Each OrderBy starts a new group placed in front of the previous ones
        // (LINQ semantics: the last OrderBy is the primary key); ThenBy appends to the first group.
        private readonly List<List<string>> _orderGroups = new List<List<string>>();

        // Non-null only while a lambda body is being translated.
        private StringBuilder _sb;

        public string GetSqlQuery(Expression node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            _whereClauses.Clear();
            _orderGroups.Clear();
            _sb = null;

            Visit(node);

            var sql = new StringBuilder("SELECT * FROM [" + TableName + "]");

            if (_whereClauses.Count > 0)
            {
                sql.Append(" WHERE ").Append(string.Join(" AND ", _whereClauses));
            }

            if (_orderGroups.Count > 0)
            {
                sql.Append(" ORDER BY ")
                   .Append(string.Join(", ", _orderGroups.SelectMany(g => g)));
            }

            SqlExpressionSeq++;
            return sql.ToString();
        }

        #region Entry point: skip parameter-independent subtrees

        /// <summary>
        /// While translating a lambda body, any subtree that doesn't reference the lambda
        /// parameter (closure variables, DateTime.Now, local lists...) is evaluated once
        /// and emitted as a SQL literal.
        /// </summary>
        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            if (_sb != null
                && !DependsOnParameter(node))
            {
                _sb.Append(FormatLiteral(Evaluate(node)));
                return node;
            }

            return base.Visit(node);
        }

        #endregion

        #region Queryable chain (Where / OrderBy / ThenBy ...)

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
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

            LambdaExpression lambda = GetLambda(node.Arguments[1]);

            switch (node.Method.Name)
            {
                case "Where":
                    _whereClauses.Add("(" + Translate(lambda, true) + ")");
                    break;

                case "OrderBy":
                    _orderGroups.Insert(0, new List<string> { Translate(lambda, false) + " ASC" });
                    break;

                case "OrderByDescending":
                    _orderGroups.Insert(0, new List<string> { Translate(lambda, false) + " DESC" });
                    break;

                case "ThenBy":
                    GetCurrentOrderGroup().Add(Translate(lambda, false) + " ASC");
                    break;

                case "ThenByDescending":
                    GetCurrentOrderGroup().Add(Translate(lambda, false) + " DESC");
                    break;

                default:
                    throw new NotSupportedException(
                        "Queryable." + node.Method.Name + " is not supported.");
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
                throw new NotSupportedException(
                    "Only single-parameter lambdas are supported (indexed overloads are not).");
            }

            return lambda;
        }

        private string Translate(LambdaExpression lambda, bool asCondition)
        {
            var previous = _sb;
            _sb = new StringBuilder();
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

                return _sb.ToString();
            }
            finally
            {
                _sb = previous;
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
                    _sb.Append(")");
                    return node;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    // "= NULL" never matches in SQL, so comparisons with null become IS [NOT] NULL.
                    string nullTest = node.NodeType == ExpressionType.Equal ? " IS NULL" : " IS NOT NULL";
                    if (IsNullValue(node.Right))
                    {
                        _sb.Append("(");
                        Visit(node.Left);
                        _sb.Append(nullTest).Append(")");
                        return node;
                    }
                    if (IsNullValue(node.Left))
                    {
                        _sb.Append("(");
                        Visit(node.Right);
                        _sb.Append(nullTest).Append(")");
                        return node;
                    }
                    break;
            }

            string op;
            if (!BinaryOperators.TryGetValue(node.NodeType, out op))
            {
                throw new NotSupportedException(
                    "Binary operator '" + node.NodeType + "' is not supported.");
            }

            _sb.Append("(");
            Visit(node.Left);
            _sb.Append(" ").Append(op).Append(" ");
            Visit(node.Right);
            _sb.Append(")");
            return node;
        }

        private void WriteLogical(BinaryExpression node, string sqlOperator)
        {
            _sb.Append("(");
            VisitCondition(node.Left);
            _sb.Append(sqlOperator);
            VisitCondition(node.Right);
            _sb.Append(")");
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
                        _sb.Append(")");
                    }
                    else
                    {
                        _sb.Append("(~");
                        Visit(node.Operand);
                        _sb.Append(")");
                    }
                    return node;

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    _sb.Append("(-");
                    Visit(node.Operand);
                    _sb.Append(")");
                    return node;

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.TypeAs:
                case ExpressionType.Quote:
                    // Conversions (e.g. int -> int?, enum -> int) are transparent in SQL.
                    Visit(node.Operand);
                    return node;

                default:
                    throw new NotSupportedException(
                        "Unary operator '" + node.NodeType + "' is not supported.");
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType != null && IsNullable(node.Member.DeclaringType))
            {
                if (node.Member.Name == "HasValue")
                {
                    _sb.Append("(");
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

            if (node.Member.DeclaringType == typeof(string) && node.Member.Name == "Length")
            {
                _sb.Append("LEN(");
                Visit(node.Expression);
                _sb.Append(")");
                return node;
            }

            if (StripConvert(node.Expression) is ParameterExpression)
            {
                _sb.Append("[").Append(node.Member.Name.Replace("]", "]]")).Append("]");
                return node;
            }

            throw new NotSupportedException(
                "Member access '" + node + "' is not supported (only direct properties of the row type).");
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Constants that are the query source (outside any lambda) produce no SQL.
            if (_sb != null)
            {
                _sb.Append(FormatLiteral(node.Value));
            }

            return node;
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

            _sb.Append("(");
            Visit(node.Object);
            _sb.Append(" LIKE ");

            if (!DependsOnParameter(argument))
            {
                var value = Evaluate(argument) as string;
                if (value == null)
                {
                    throw new NotSupportedException("A null pattern is not supported in " + name + ".");
                }

                _sb.Append(FormatString(prefix + EscapeLike(value) + suffix));
            }
            else
            {
                // Column-to-column pattern: concatenate the wildcards (no escaping possible).
                if (prefix.Length > 0) _sb.Append("N'").Append(prefix).Append("' + ");
                Visit(argument);
                if (suffix.Length > 0) _sb.Append(" + N'").Append(suffix).Append("'");
            }

            _sb.Append(")");
        }

        private void WriteIn(MethodCallExpression node)
        {
            // Either list.Contains(item) (instance) or Enumerable.Contains(list, item) (static).
            Expression collection = node.Object ?? node.Arguments[0];
            Expression item = node.Object != null ? node.Arguments[0] : node.Arguments[1];

            if (DependsOnParameter(collection))
            {
                throw new NotSupportedException("Contains on a row member is not supported.");
            }

            var values = Evaluate(collection) as IEnumerable;
            if (values == null)
            {
                throw new NotSupportedException("Contains requires a non-null collection.");
            }

            List<string> literals = values.Cast<object>().Select(FormatLiteral).ToList();
            if (literals.Count == 0)
            {
                _sb.Append("(1 = 0)");
                return;
            }

            _sb.Append("(");
            Visit(item);
            _sb.Append(" IN (").Append(string.Join(", ", literals)).Append("))");
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

        private static bool IsPredicate(Expression node)
        {
            if (!DependsOnParameter(node))
            {
                return false; // evaluated to a 1/0 literal
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
                           && IsNullable(member.Member.DeclaringType);

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
            return method.DeclaringType == typeof(string)
                   && (method.Name == "Contains" || method.Name == "StartsWith" || method.Name == "EndsWith")
                   && method.GetParameters().Length == 1
                   && method.GetParameters()[0].ParameterType == typeof(string);
        }

        private static bool IsInMethod(MethodInfo method)
        {
            return method.Name == "Contains" && method.DeclaringType != typeof(string);
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
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

        private static bool IsNullValue(Expression node)
        {
            node = StripConvert(node);
            var constant = node as ConstantExpression;
            if (constant != null)
            {
                return constant.Value == null;
            }

            return !DependsOnParameter(node) && Evaluate(node) == null;
        }

        private static bool DependsOnParameter(Expression node)
        {
            var finder = new ParameterFinder();
            finder.Visit(node);
            return finder.Found;
        }

        private static object Evaluate(Expression node)
        {
            return Expression.Lambda<Func<object>>(Expression.Convert(node, typeof(object))).Compile()();
        }

        private static string EscapeLike(string value)
        {
            // Bracket-escape LIKE wildcards; '[' must be replaced first.
            return value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        private static string FormatString(string value)
        {
            return "N'" + value.Replace("'", "''") + "'";
        }

        private static string FormatLiteral(object value)
        {
            switch (value)
            {
                case null:
                    return "NULL";
                case string s:
                    return FormatString(s);
                case char c:
                    return FormatString(c.ToString());
                case bool b:
                    return b ? "1" : "0";
                case Enum e:
                    return FormatLiteral(Convert.ChangeType(e, Enum.GetUnderlyingType(e.GetType()), CultureInfo.InvariantCulture));
                case DateTime dt:
                    return "'" + dt.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "'";
                case DateTimeOffset dto:
                    return "'" + dto.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture) + "'";
                case Guid g:
                    return "'" + g.ToString("D") + "'";
                case byte[] bytes:
                    return "0x" + BitConverter.ToString(bytes).Replace("-", string.Empty);
                case float f:
                    return f.ToString("R", CultureInfo.InvariantCulture);
                case double d:
                    return d.ToString("R", CultureInfo.InvariantCulture);
                case byte _:
                case sbyte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case ulong _:
                case decimal _:
                    return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
                default:
                    throw new NotSupportedException(
                        "Constant of type '" + value.GetType().FullName + "' is not supported.");
            }
        }

        private sealed class ParameterFinder : ExpressionVisitor
        {
            public bool Found { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                Found = true;
                return node;
            }
        }

        #endregion
    }
}