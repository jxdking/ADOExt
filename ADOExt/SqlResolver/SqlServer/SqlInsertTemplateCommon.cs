using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.SqlServer
{
    public class SqlInsertTemplateCommon<T, TParameter> : SqlInsertTemplateBase<T>
        where TParameter : IDbDataParameter, new()
    {
        private string Template;
        private string TemplateAllCol;
        private int ColCount;

        public SqlInsertTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
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
            return new Sql(sqltxt, cols.Select(i => (IDbDataParameter)new TParameter
            {
                ParameterName = i.ColumnName,
                Value = i.PropertyGetter(obj),
            }));
        }
    }
}
