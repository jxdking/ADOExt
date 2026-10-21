using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MagicEastern.ADOExt.IQueryable
{
    public class SqlQueryProvider : IQueryProvider
    {
        
        
        private readonly System.Linq.IQueryable _source;

        public SqlQueryProvider(System.Linq.IQueryable source)
        {
            _source = source;
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

        public object Execute(Expression expression)
        {
            return Execute<object>(expression);
        }

        
        public TResult Execute<TResult>(Expression expression)
        {
            Console.WriteLine($"[LINQ] Executing: {expression}");

            // Rewrite the expression to use our inner source
            return _source.Provider.Execute<TResult>(expression);
        }



        #endregion

        
        public string Execute2(Expression expression) {
            throw new NotImplementedException();
        }

    }
}
