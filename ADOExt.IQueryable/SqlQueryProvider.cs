using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MagicEastern.ADOExt.IQueryable
{
    public class SqlQueryProvider : IQueryProvider
    {
        private readonly Sql _sql;
        private readonly DBConnectionWrapper _conn;
        private Func<string, string> _getColumnNameFunc;
        private string _paraSymbol;

        public SqlQueryProvider(Sql sql, DBConnectionWrapper conn, Func<string, string> getColumnNameFunc, string paraSymbol)
        {
            _sql = sql;
            _conn = conn;
            _getColumnNameFunc = getColumnNameFunc;
            _paraSymbol = paraSymbol;
        }


        #region IQueryProvider
        public System.Linq.IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetSequenceElementType();
            var queryableType = typeof(SqlQueryable<>).MakeGenericType(elementType);
            var ctor = queryableType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(SqlQueryProvider), typeof(Expression) },
                null);
            return (System.Linq.IQueryable)ctor.Invoke(new object[] { this, expression });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SqlQueryable<TElement>(this, expression);
        }


        private static List<T> QueryAsList<T>(Sql sql, DBConnectionWrapper conn) where T : new()
        {
            return conn.Query<T>(sql).ToList();
        }

        public Sql ToSql(Expression expression) {
            var visitor = new SqlExpressionVisitor();
            var ret = visitor.GetSqlQuery(expression, _sql, _getColumnNameFunc, _paraSymbol);
            return ret;
        }


        public object Execute(Expression expression)
        {
            var elementType = expression.Type.GetSequenceElementType();
            var mi = typeof(SqlQueryProvider).GetMethod(nameof(QueryAsList), BindingFlags.Static | BindingFlags.NonPublic);
            var generic = mi.MakeGenericMethod(elementType);

            var finalSql = ToSql(expression);
            var ret = generic.Invoke(null, new object[] { finalSql, _conn });
            return ret;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }
        #endregion


    }
}
