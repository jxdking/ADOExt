using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public interface ISqlUpdateTemplate<T>
    {
        int Execute(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
        Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, string parameterSuffix = null);
    }
}