using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public abstract class SqlUpdateTemplateBase<T> : ISqlUpdateTemplate<T>
    {
        public abstract Sql Generate(T obj, IEnumerable<IDBColumnMapping<T>> setCols);

        public abstract int Execute(T obj, IEnumerable<IDBColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
    }
}
