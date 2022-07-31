using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public interface IDBTableMapping<T>
    {
        List<IDBTableColumnMapping<T>> ColumnMappingList { get; }
        string Schema { get; }
        string TableName { get; }
    }
}