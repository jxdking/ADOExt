using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.Oracle
{
    public class SqlUpdateTemplateCommon<T, TParameter> : SqlUpdateTemplateBase<T> 
        where T : new()
        where TParameter : IDbDataParameter, new()
    {
        private readonly DBTableAdapterContext<T> context;
        private string Template;
        private string TemplateAllCol;
        private int ColCount;
        private List<IDBColumnMapping<T>> ReturnCols;

        public SqlUpdateTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) 
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            ReturnCols = SqlTemplateUtil.GetReturningCols(context).ToList();

            Template = "begin\r\n";
            Template += "update " + tablename + " set {0} where " + string.Join(" and ", context.PkColumnsInfo.Select(i => i.ColumnName + "=:" + i.ColumnName)) + " returning " + string.Join(",", ReturnCols.Select(i => i.ColumnName)) + " into :" + string.Join(",:", ReturnCols.Select(i => i.ColumnName)) + ";\r\n";
            Template += ":sql_nor := sql%ROWCOUNT;\r\n";
            Template += "end;";

            TemplateAllCol = string.Format(Template, string.Join(",", context.SetColumnsInfo.Select(i => i + "=:" + i)));
            ColCount = context.SetColumnsInfo.Count;
            this.context = context;
        }


        public override int Execute(T obj, IEnumerable<IDBColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            var sql = Generate(obj, setCols);
            conn.Execute(sql, out var outParas, false, trans);
            var paraDic = outParas.ToDictionary(i => i.ParameterName, i => DBNull.Value.Equals(i.Value) ? null : i.Value);
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
            return SqlTemplateUtil.GenerateSql<T, TParameter>(obj, sqltxt, cols.Concat(context.PkColumnsInfo), ReturnCols);
        }
    }
}
