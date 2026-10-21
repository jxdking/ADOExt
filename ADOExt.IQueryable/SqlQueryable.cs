using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MagicEastern.ADOExt.IQueryable
{
    public class SqlQueryable<T> : IQueryable<T>, IOrderedQueryable<T>
    {
        private readonly SqlQueryProvider _providor;
        private readonly Expression _expression;

        public SqlQueryable(SqlQueryProvider provider)
        {
            _providor = provider;
            _expression = Expression.Constant(this);
        }

        internal SqlQueryable(SqlQueryProvider provider, Expression expression)
        {
            _providor = provider;
            _expression = expression;
        }

        #region IQueryable
        public Type ElementType => typeof(T);

        public Expression Expression => _expression;

        public IQueryProvider Provider => _providor;

        public IEnumerator<T> GetEnumerator()
        {
            return _providor.Execute<IEnumerable<T>>(_expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
