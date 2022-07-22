using System;
using System.Data;
using System.Linq.Expressions;

namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapter<T> where T : new()
    {
        void ApplyMaxLength(T entity);
        int Delete(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null);
        int Insert(T obj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null);
        T Load(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null);
        int Update(T obj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null, params Expression<Func<T, object>>[] targetProperties);
    }
}