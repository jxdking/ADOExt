using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlLoadTemplateBase<T, TParameter> : ISqlLoadTemplate<T>
        where TParameter : IDbDataParameter, new()
    {
        private string Template;
        private IEnumerable<IDBColumnMapping<T>> PkCols;

        public SqlLoadTemplateBase(IEnumerable<IDBColumnMapping<T>> pkCols, string template)
        {
            PkCols = pkCols;
            Template = template;
        }

        public Sql Generate(T obj)
        {
            var sql = new Sql(Template, PkCols.Select(i =>
            {
                IDbDataParameter p = new TParameter();
                p.ParameterName = i.ColumnName;
                p.Value = i.PropertyGetter(obj);
                return p;
            }));
            sql.SchemaCachePriority = CacheItemPriority.NeverRemove;
            return sql;
        }
    }
}
