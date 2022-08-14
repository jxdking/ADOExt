using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlDeleteTemplateBase<T, TParameter> : ISqlDeleteTemplate<T>
        where TParameter : IDbDataParameter, new()
    {
        private string Template;
        private IEnumerable<IDBColumnMapping<T>> PkCols;

        protected SqlDeleteTemplateBase(IEnumerable<IDBColumnMapping<T>> pkCols, string template)
        {
            Template = template;
            PkCols = pkCols;
        }

        public Sql Generate(T obj)
        {
            var sql = new Sql(Template, PkCols.Select(i =>
            {
                var p = new TParameter();
                p.ParameterName = i.ColumnName;
                p.Value = i.PropertyGetter(obj);
                return p;
            }));
            return sql;
        }
    }
}
