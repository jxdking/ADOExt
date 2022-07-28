using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public abstract class SqlUpdateTemplateBase<T>
    {
        protected readonly DBTableAdapterContext<T> context;
        protected IEnumerable<IDBColumnMapping<T>> PkCols;

        public SqlUpdateTemplateBase(DBTableAdapterContext<T> context)
        {
            PkCols = context.PkColumnsInfo;
            this.context = context;
        }

        public abstract Sql Generate(T obj, IEnumerable<IDBColumnMapping<T>> setCols);

        public abstract int Execute(T obj, IEnumerable<IDBColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
    }
}
