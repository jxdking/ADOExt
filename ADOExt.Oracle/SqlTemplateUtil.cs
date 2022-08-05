using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.Oracle
{
    internal static class SqlTemplateUtil
    {
        internal static readonly string RowCountParaName = "sql_nor";

        internal static IEnumerable<IDBColumnMapping<T>> GetReturningCols<T>(DBTableAdapterContext<T> context)
        {
            var returningCols = context.Mapping.ColumnMappingList.Where(i => i.DataType != "LONG" && i.DataType != "CLOB");
            return returningCols;
        }

        internal static Sql GenerateSql<T>(T obj, string sqltxt, IEnumerable<IDBColumnMapping<T>> inputCols, IEnumerable<IDBColumnMapping<T>> returnCols)
        {
            var sql = new Sql(sqltxt, returnCols.Select(i => new OracleParameter
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
                    p = new OracleParameter
                    {
                        ParameterName = c.ColumnName,
                        Value = c.PropertyGetter(obj),
                        Direction = System.Data.ParameterDirection.Input
                    };
                    sql.Parameters.Add(p);
                }
            }

            sql.Parameters.Add(new OracleParameter { ParameterName = RowCountParaName, Value = -1, Direction = System.Data.ParameterDirection.Output });
            return sql;
        }
    }
}
