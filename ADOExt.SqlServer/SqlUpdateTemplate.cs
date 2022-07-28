using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlUpdateTemplate<T> : SqlUpdateTemplateBase<T> where T : new()
    {
        private string Template;

        public SqlUpdateTemplate(DBTableAdapterContext<T> context, SqlResolver sqlResolver) : base(context)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            Template = "update " + tablename + " set {0} where " + string.Join(" and ", PkCols.Select(i => "[" + i + "]=@" + i));
        }

        public override int Execute(T obj, IEnumerable<IDBColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            var sql = Generate(obj, setCols);
            result = default(T);
            return conn.Execute(sql, false, trans);
        }

        public override Sql Generate(T obj, IEnumerable<IDBColumnMapping<T>> setCols)
        {
            if (!(setCols is List<IDBColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }
            var sqltxt = string.Format(Template, string.Join(",", cols.Select(i => "[" + i + "]=@" + i)));
            return new Sql(sqltxt, cols.Concat(PkCols).Select(i => new Parameter
            {
                Name = i.ColumnName,
                Value = i.PropertyGetter(obj),
                ObjectType = i.ObjectProperty.PropertyType
            }));
        }
    }
}
