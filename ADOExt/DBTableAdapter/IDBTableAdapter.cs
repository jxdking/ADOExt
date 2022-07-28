namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapter<T> where T : new()
    {
        void ApplyMaxLength(T entity);
        int Delete(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans);
        int Insert(T obj, object properties, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
        T Load(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans);
        int Update(T obj, object properties, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
    }
}