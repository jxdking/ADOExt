namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapter<T> where T : new()
    {
        void ApplyMaxLength(T entity);

        Sql GetDeleteSql(T obj, string parameterSuffix = null);
        int Delete(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans);
        
        Sql GetInsertSql(T obj, object properties, string parameterSuffix = null);
        int Insert(T obj, object properties, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
        
        T Load(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans);
        
        Sql GetUpdateSql(T obj, object properties, string parameterSuffix = null);
        int Update(T obj, object properties, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
        
    }
}