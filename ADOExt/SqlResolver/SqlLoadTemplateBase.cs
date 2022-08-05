using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlLoadTemplateBase<T>
    {
        private string Template;
        private IEnumerable<IDBColumnMapping<T>> PkCols;
        private IDBCommandBuilder CommandBuilder;

        public SqlLoadTemplateBase(IEnumerable<IDBColumnMapping<T>> pkCols, IDBCommandBuilder commandBuilder, string template)
        {
            PkCols = pkCols;
            CommandBuilder = commandBuilder;
            Template = template;
        }

        public Sql Generate(T obj)
        {
            var sql = new Sql(Template, PkCols.Select(i => {
                var p = CommandBuilder.CreateParameter();
                p.ParameterName = i.ColumnName;
                p.Value = i.PropertyGetter(obj);
                return p;
            }));
            sql.CacheDataReaderSchema = true;
            return sql;
        }
    }
}
