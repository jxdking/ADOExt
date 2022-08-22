using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.Oracle
{
    internal static class SqlTemplateUtil
    {
        internal static readonly string RowCountParaName = "sql_nor";

        internal static IEnumerable<IDBColumnMapping<T>> GetReturningCols<T>(DBTableAdapterContext<T> context)
        {
            var returningCols = context.Mapping.ColumnMappingList.Where(i => i.DataType != "LONG" && i.DataType != "CLOB");
            return returningCols;
        }

        internal static Sql GenerateSql<T, TParameter>(T obj, string sqltxt, IEnumerable<IDBColumnMapping<T>> inputCols, IEnumerable<IDBColumnMapping<T>> returnCols)
            where TParameter : IDbDataParameter, new()
        {
            var sql = new Sql(sqltxt, returnCols.Select(i => (IDbDataParameter)new TParameter
            {
                ParameterName = i.ColumnName,
                Direction = System.Data.ParameterDirection.Output,
                DbType = i.DbType
            }));

            foreach (var c in inputCols)
            {
                if (sql.Parameters.TryGetValue(c.ColumnName, out var p)) {
                    p.Direction = System.Data.ParameterDirection.InputOutput;
                    p.DbType = c.DbType;
                    p.Value = c.PropertyGetter(obj);
                } else {
                    p = new TParameter
                    {
                        ParameterName = c.ColumnName,
                        Value = c.PropertyGetter(obj),
                        Direction = System.Data.ParameterDirection.Input
                    };
                    sql.Parameters.Add(p);
                }
            }

            sql.Parameters.Add(new TParameter { ParameterName = RowCountParaName, Value = -1, Direction = System.Data.ParameterDirection.Output });
            return sql;
        }
    }
}
