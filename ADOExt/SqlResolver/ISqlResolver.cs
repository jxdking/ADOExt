namespace MagicEastern.ADOExt
{
    /// <summary>
    /// as an extension anchor
    /// </summary>
    public interface ISqlResolver
    {
        string GetTableName(string table, string schema);
        Sql ColumnMetaDataFromTable(string table, string schema);
        ISqlInsertTemplate<T> GetInsertTemplate<T>() where T : new();
        ISqlUpdateTemplate<T> GetUpdateTemplate<T>() where T : new();
        ISqlLoadTemplate<T> GetLoadTemplate<T>() where T : new();
        ISqlDeleteTemplate<T> GetDeleteTemplate<T>() where T : new();
    }
}