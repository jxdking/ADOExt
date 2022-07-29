using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt.Oracle
{
    public class SqlUpdateTemplate<T> : SqlUpdateTemplateBase<T> where T : new()
    {
        private string Template;
        private string TemplateAllCol;
        private int ColCount;
        private List<IDBColumnMapping<T>> ReturnCols;

        public SqlUpdateTemplate(DBTableAdapterContext<T> context, SqlResolver sqlResolver) : base(context)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            ReturnCols = SqlTemplateUtil.GetReturningCols(context).ToList();

            Template = "begin\r\n";
            Template += "update " + tablename + " set {0} where " + string.Join(" and ", PkCols.Select(i => i + "=:" + i)) + " returning " + string.Join(",", ReturnCols.Select(i => i.ColumnName)) + " into :" + string.Join(",:", ReturnCols.Select(i => i.ColumnName)) + ";\r\n";
            Template += ":sql_nor := sql%ROWCOUNT;\r\n";
            Template += "end;";

            TemplateAllCol = string.Format(Template, string.Join(",", context.SetColumnsInfo.Select(i => i + "=:" + i)));
            ColCount = context.SetColumnsInfo.Count;
        }


        public override int Execute(T obj, IEnumerable<IDBColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            var sql = Generate(obj, setCols);
            conn.Execute(sql, out var outParas, false, trans);
            var paraDic = outParas.ToDictionary(i => i.Name, i => i.Value);
            result = context.AllColumnsInfo.Parse(paraDic);
            return (int)paraDic[SqlTemplateUtil.RowCountParaName];
        }

        public override Sql Generate(T obj, IEnumerable<IDBColumnMapping<T>> setCols)
        {
            if (!(setCols is List<IDBColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }
            var sqltxt = cols.Count == ColCount ? TemplateAllCol
                : string.Format(Template, string.Join(",", cols.Select(i => string.Join("", new string[] { i.ColumnName, "=:", i.ColumnName }))));
            return SqlTemplateUtil.GenerateSql(obj, sqltxt, cols.Concat(PkCols), ReturnCols);
        }
    }
}
