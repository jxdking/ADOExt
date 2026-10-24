using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MagicEastern.ADOExt.IQueryable
{
    public static class SqlExt
    {
        private static bool IsOracle(this IDBService db)
        {
            string typename = db.SqlResolver.GetType().FullName;
            return typename == "MagicEastern.ADOExt.Oracle.SqlResolver"
                || typename == "MagicEastern.ADOExt.Oracle.Core.SqlResolver";
        }

        private static bool IsSqlServer(this IDBService db)
        {
            return db.SqlResolver.GetType().FullName == "MagicEastern.ADOExt.SqlServer.SqlResolver";
        }

        public static IQueryable<T> AsQueryable<T>(this Sql sql, DBConnectionWrapper conn) {
            var colmapping = conn.DBService.GetDBObjectMapping<T>().ColumnMappingList.ToDictionary(i => i.ObjectProperty.Name, i => i.ColumnName);

            string paraSymbol;
            if (conn.DBService.IsSqlServer()) {
                paraSymbol = "@";
            } else if (conn.DBService.IsOracle()) {
                paraSymbol = ":";
            } else {
                throw new NotSupportedException();
            }

            var provider = new SqlQueryProvider(sql, conn, (n) => colmapping[n], paraSymbol);

            var ret = new SqlQueryable<T>(provider);
            return ret;
        }
    }
}
