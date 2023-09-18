using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.Oracle
{
    internal static class SqlTemplateUtil
    {
        internal static readonly string RowCountParaName = "sql_nor";

        internal static IEnumerable<IDBTableColumnMapping<T>> GetReturningCols<T>(DBTableAdapterContext<T> context)
        {
            var returningCols = context.Mapping.ColumnMappingList.Where(i => i.DataType != "LONG" && i.DataType != "CLOB");
            return returningCols;
        }

        internal static Sql GenerateSql<T, TParameter>(T obj, string sqltxt, IEnumerable<IDBTableColumnMapping<T>> inputCols, IEnumerable<IDBTableColumnMapping<T>> returnCols
            , ISqlResolver sqlResolver, string parameterSuffix = null)
            where TParameter : IDbDataParameter, new()
        {
            var sql = new Sql(sqltxt, returnCols.Select(i => (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Output, parameterSuffix)));

            foreach (var c in inputCols)
            {
                if (sql.Parameters.TryGetValue(c.ColumnName + parameterSuffix, out var p)) {
                    sqlResolver.ConfigureParameter(p, c, obj, ParameterDirection.InputOutput, parameterSuffix);
                } else {
                    p = sqlResolver.CreateParameter<T, TParameter>(c, obj, ParameterDirection.Input, parameterSuffix);
                    sql.Parameters.Add(p);
                }
            }

            sql.Parameters.Add(new TParameter { ParameterName = RowCountParaName, Value = -1, Direction = ParameterDirection.Output });
            return sql;
        }
    }
}
