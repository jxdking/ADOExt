using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public interface ISqlInsertTemplate<T>
    {
        int Execute(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn);
        Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, string parameterSuffix = null);
    }
}