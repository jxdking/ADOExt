using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlLoadTemplateBase<T>
    {
        protected readonly DBTableAdapterContext<T> context;

        /// <summary>
        /// e.g.
        /// select * from table_name where id = @id;
        /// </summary>
        protected string Template;
        protected IEnumerable<IDBColumnMapping<T>> PkCols;

        public SqlLoadTemplateBase(DBTableAdapterContext<T> context)
        {
            PkCols = context.PkColumnsInfo;
            this.context = context;
        }

        public Sql Generate(T obj)
        {
            var sql = new Sql(Template, PkCols.Select(i => new Parameter
            {
                Name = i.ColumnName,
                Value = i.PropertyGetter(obj),
                ObjectType = i.ObjectProperty.PropertyType
            }));
            sql.CacheDataReaderSchema = true;
            return sql;
        }
    }
}
