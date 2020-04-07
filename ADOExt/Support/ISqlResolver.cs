using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    /// <summary>
    /// as an extension anchor
    /// </summary>
    public interface ISqlResolver
    {
        Sql ColumnMetaDataFromTable(string table, string schema = null);

        string InsertTemplate<T>(DBTableAdapterContext<T> context);

        string DeleteTemplate<T>(DBTableAdapterContext<T> context);

        string UpdateTemplate<T>(DBTableAdapterContext<T> context);

        string LoadTemplate<T>(DBTableAdapterContext<T> context);
    }
}