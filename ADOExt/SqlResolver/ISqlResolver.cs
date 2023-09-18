using System.Data;
using System.Xml.Schema;

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
        void ConfigureParameter<T>(IDbDataParameter p, IDBTableColumnMapping<T> col, T obj, ParameterDirection direction, string parameterSuffix = null);
    }

    public static class ISqlResolverExtension
    {
        public static TParameter CreateParameter<T, TParameter>(this ISqlResolver sqlResolver, IDBTableColumnMapping<T> col, T obj, ParameterDirection direction, string parameterSuffix = null) where TParameter : IDbDataParameter, new()
        {
            TParameter p = new TParameter();
            sqlResolver.ConfigureParameter(p, col, obj, direction, parameterSuffix);
            return p;
        }
    }
}