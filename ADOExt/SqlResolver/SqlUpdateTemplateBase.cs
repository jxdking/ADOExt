using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public abstract class SqlUpdateTemplateBase<T> : ISqlUpdateTemplate<T>
    {
        public abstract Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols);

        public abstract int Execute(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
    }
}
