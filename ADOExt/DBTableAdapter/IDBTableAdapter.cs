namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapter<T> where T : new()
    {
        void ApplyMaxLength(T entity);

        Sql GetDeleteSql(T obj, string parameterSuffix = null);
        int Delete(T obj, DBConnectionWrapper conn);
        
        Sql GetInsertSql(T obj, object properties, string parameterSuffix = null);
        int Insert(T obj, object properties, out T result, DBConnectionWrapper conn);
        
        T Load(T obj, DBConnectionWrapper conn);
        
        Sql GetUpdateSql(T obj, object properties, string parameterSuffix = null);
        int Update(T obj, object properties, out T result, DBConnectionWrapper conn);
        
    }
}