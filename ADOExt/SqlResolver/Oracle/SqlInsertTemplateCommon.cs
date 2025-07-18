using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace MagicEastern.ADOExt.Common.Oracle
{
    public class SqlInsertTemplateCommon<T, TParameter> : SqlInsertTemplateBase<T>
        where TParameter : IDbDataParameter, new()
        where T : new()
    {
        private readonly DBTableAdapterContext<T> context;
        private readonly ISqlResolver sqlResolver;
        private string Template;
        private string TemplateAllCol;
        private int ColCount;
        private List<IDBTableColumnMapping<T>> ReturnCols;
        public SqlInsertTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            ReturnCols = SqlTemplateUtil.GetReturningCols(context).ToList();

            Template = "begin\r\n";
            Template += "insert into " + tablename + "({0}) values ({1}) returning " + string.Join(",", ReturnCols.Select(i => i.ColumnName)) + " into :" + string.Join(",:", ReturnCols.Select(i => i.ColumnName)) + ";\r\n";
            Template += ":sql_nor := sql%ROWCOUNT;\r\n";
            Template += "end;";

            TemplateAllCol = string.Format(Template, string.Join(",", context.InsertColumnsInfo.Select(i => i.ColumnName)), ":" + string.Join(",:", context.InsertColumnsInfo.Select(i => i.ColumnName)));
            ColCount = context.InsertColumnsInfo.Count;
            this.context = context;
            this.sqlResolver = sqlResolver;
        }

        public override int Execute(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn)
        {
            var sql = Generate(obj, setCols);
            conn.Execute(sql, out var outParas, false);
            var paraDic = outParas.ToDictionary(i => i.ParameterName, i => DBNull.Value.Equals(i.Value) ? null : i.Value);

            result = context.AllColumnsInfo.Parse(paraDic);
            return (int)paraDic[SqlTemplateUtil.RowCountParaName];
        }

        public override Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, string parameterSuffix = null)
        {
            if (!string.IsNullOrWhiteSpace(parameterSuffix))
            {
                throw new NotSupportedException("Not null parameterSuffix is not supported.");
            }


            if (!(setCols is List<IDBTableColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }
            string sqltxt = cols.Count == ColCount ? TemplateAllCol
                : string.Format(Template, string.Join(",", cols.Select(i => i.ColumnName)), ":" + string.Join(",:", cols.Select(i => i.ColumnName)));

            return SqlTemplateUtil.GenerateSql<T, TParameter>(obj, sqltxt, cols, ReturnCols, sqlResolver);
        }
    }
}
