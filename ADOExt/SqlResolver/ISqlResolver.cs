namespace MagicEastern.ADOExt
{
    /// <summary>
    /// as an extension anchor
    /// </summary>
    public interface ISqlResolver
    {
        string GetTableName(string table, string schema);
        Sql ColumnMetaDataFromTable(string table, string schema);
        SqlInsertTemplateBase<T> GetInsertTemplate<T>() where T : new();
        SqlUpdateTemplateBase<T> GetUpdateTemplate<T>() where T : new();
        SqlLoadTemplateBase<T> GetLoadTemplate<T>() where T : new();
        SqlDeleteTemplateBase<T> GetDeleteTemplate<T>() where T : new();
    }
}