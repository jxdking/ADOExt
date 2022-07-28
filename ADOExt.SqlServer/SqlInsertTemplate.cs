using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlInsertTemplate<T> : SqlInsertTemplateBase<T> where T : new()
    {
        private string Template;
        private string TemplateAllCol;
        private int ColCount;

        public SqlInsertTemplate(DBTableAdapterContext<T> context, SqlResolver sqlResolver) : base(context)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            Template = "insert into " + tablename + "([{0}]) values ({1})";

            TemplateAllCol = string.Format(Template, string.Join("],[", context.InsertColumnsInfo.Select(i => i.ColumnName)), "@" + string.Join(",@", context.InsertColumnsInfo.Select(i => i.ColumnName)));
            ColCount = context.InsertColumnsInfo.Count;
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
            string sqltxt = cols.Count == ColCount ? TemplateAllCol
                : string.Format(Template, string.Join("],[", cols.Select(i => i.ColumnName)), "@" + string.Join(",@", cols.Select(i => i.ColumnName)));
            return new Sql(sqltxt, cols.Select(i => new Parameter
            {
                Name = i.ColumnName,
                Value = i.PropertyGetter(obj),
                ObjectType = i.ObjectProperty.PropertyType
            }));
        }
    }
}
