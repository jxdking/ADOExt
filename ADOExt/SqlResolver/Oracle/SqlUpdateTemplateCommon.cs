﻿using System;
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
        private readonly ISqlResolver sqlResolver;
        private string Template;
        private string TemplateAllCol;
        private int ColCount;
        private List<IDBTableColumnMapping<T>> ReturnCols;

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
            this.sqlResolver = sqlResolver;
        }


        public override int Execute(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn)
        {
            var sql = Generate(obj, setCols);
            conn.Execute(sql, out var outParas, false);
            var paraDic = outParas.ToDictionary(i => i.ParameterName, i => DBNull.Value.Equals(i.Value) ? null : i.Value);
            result = ((int)paraDic["sql_nor"] == 0) ? default(T) : context.AllColumnsInfo.Parse(paraDic);

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
            var sqltxt = cols.Count == ColCount ? TemplateAllCol
                : string.Format(Template, string.Join(",", cols.Select(i => i.ColumnName + "=:" + i.ColumnName)));
            return SqlTemplateUtil.GenerateSql<T, TParameter>(obj, sqltxt, cols.Concat(context.PkColumnsInfo), ReturnCols, sqlResolver);
        }
    }
}
