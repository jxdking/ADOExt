﻿using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.Oracle
{
    public class SqlInsertTemplate<T> : SqlInsertTemplateBase<T> where T : new()
    {
        private string Template;
        private string TemplateAllCol;
        private int ColCount;
        private List<IDBColumnMapping<T>> ReturnCols;
        public SqlInsertTemplate(DBTableAdapterContext<T> context, SqlResolver sqlResolver) : base(context)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            ReturnCols = SqlTemplateUtil.GetReturningCols(context).ToList();

            Template = "begin\r\n";
            Template += "insert into " + tablename + "({0}) values ({1}) returning " + string.Join(",", ReturnCols.Select(i => i.ColumnName)) + " into :" + string.Join(",:", ReturnCols.Select(i => i.ColumnName)) + ";\r\n";
            Template += ":sql_nor := sql%ROWCOUNT;\r\n";
            Template += "end;";

            TemplateAllCol = string.Format(Template, string.Join(",", context.InsertColumnsInfo.Select(i => i.ColumnName)), ":" + string.Join(",:", context.InsertColumnsInfo.Select(i => i.ColumnName)));
            ColCount = context.InsertColumnsInfo.Count;
        }

        private T ParseResult(Dictionary<string, object> paraDic)
        {
            return context.AllColumnsInfo.Parse(paraDic);
        }

        public override int Execute(T obj, IEnumerable<IDBColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            var sql = Generate(obj, setCols);
            conn.Execute(sql, out var outParas, false, trans);
            var paraDic = outParas.ToDictionary(i => i.Name, i => i.Value);
            result = ParseResult(paraDic);
            return (int)paraDic[SqlTemplateUtil.RowCountParaName];
        }

        public override Sql Generate(T obj, IEnumerable<IDBColumnMapping<T>> setCols)
        {
            if (!(setCols is List<IDBColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }
            string sqltxt = cols.Count == ColCount ? TemplateAllCol
                : string.Format(Template, string.Join(",", cols.Select(i => i.ColumnName)), ":" + string.Join(",:", cols.Select(i => i.ColumnName)));

            return SqlTemplateUtil.GenerateSql(obj, sqltxt, cols, ReturnCols);
        }
    }
}