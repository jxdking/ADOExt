using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public abstract class SqlUpdateTemplateBase<T> : ISqlUpdateTemplate<T>
    {
        public abstract Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, string parameterSuffix = null);

        public virtual int Execute(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn)
        {
            var sql = Generate(obj, setCols);
            result = default(T);
            return conn.Execute(sql, false);
        }
    }
}
