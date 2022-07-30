using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public interface IDBTableMapping<T>
    {
        IReadOnlyList<IDBTableColumnMapping<T>> ColumnMappingList { get; }
        string Schema { get; }
        string TableName { get; }
    }
}