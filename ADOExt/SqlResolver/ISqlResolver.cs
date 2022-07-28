namespace MagicEastern.ADOExt
{
    /// <summary>
    /// as an extension anchor
    /// </summary>
    public interface ISqlResolver
    {
        Sql ColumnMetaDataFromTable(string table, string schema);
        SqlInsertTemplateBase<T> GetInsertTemplate<T>(DBTableAdapterContext<T> context) where T : new();
        SqlUpdateTemplateBase<T> GetUpdateTemplate<T>(DBTableAdapterContext<T> context) where T : new();
        SqlLoadTemplateBase<T> GetLoadTemplate<T>(DBTableAdapterContext<T> context) where T : new();
        SqlDeleteTemplateBase<T> GetDeleteTemplate<T>(DBTableAdapterContext<T> context) where T : new();
    }
}