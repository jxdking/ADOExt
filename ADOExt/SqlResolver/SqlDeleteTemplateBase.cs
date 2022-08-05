using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlDeleteTemplateBase<T>
    {
        private IDBCommandBuilder CommandBuilder;
        private string Template;
        private IEnumerable<IDBColumnMapping<T>> PkCols;
        
        protected SqlDeleteTemplateBase(IEnumerable<IDBColumnMapping<T>> pkCols, IDBCommandBuilder commandBuilder, string template)
        {
            CommandBuilder = commandBuilder;
            Template = template;
            PkCols = pkCols;
        }

        public Sql Generate(T obj)
        {
            var sql = new Sql(Template, PkCols.Select(i => {
                var p = CommandBuilder.CreateParameter();
                p.ParameterName = i.ColumnName;
                p.Value = i.PropertyGetter(obj);
                return p;
            }));
            return sql;
        }
    }
}
