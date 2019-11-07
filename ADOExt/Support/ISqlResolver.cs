using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    /// <summary>
    /// as an extension anchor
    /// </summary>
    public interface ISqlResolver
    {
        string DataBaseType { get; }

        Sql ColumnMetaDataFromTable(string table, string schema = null);

        string InsertTemplate(string table, IEnumerable<string> insertColumns, IEnumerable<string> returningColumns, string schema = null);

        string DeleteTemplate(string table, IEnumerable<string> pkColumns, string schema = null);

        string UpdateTemplate(string table, IEnumerable<string> pkColumns, IEnumerable<string> setColumns, IEnumerable<string> returningColumns, string schema = null);

        string LoadTemplate(string table, IEnumerable<string> pkColumns, IEnumerable<string> selectColumns, string schema = null);
    }
}