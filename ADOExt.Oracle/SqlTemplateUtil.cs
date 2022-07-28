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

            var sql = new Sql(sqltxt, returnCols.Select(i => new Parameter
            {
                Name = i.ColumnName,
                ObjectType = i.ObjectProperty.PropertyType,
                Direction = System.Data.ParameterDirection.Output
            }));

            foreach (var c in inputCols)
            {
                var p = new Parameter
                {
                    Name = c.ColumnName,
                    Value = c.PropertyGetter(obj),
                    ObjectType = c.ObjectProperty.PropertyType,
                    Direction = System.Data.ParameterDirection.Input
                };
                if (sql.Parameters.Remove(p))
                {
                    p.Direction = System.Data.ParameterDirection.InputOutput;
                }
                sql.Parameters.Add(p);
            }

            sql.Parameters.Add(new Parameter { Name = RowCountParaName, Value = -1, Direction = System.Data.ParameterDirection.Output });
            return sql;
        }
    }
}
