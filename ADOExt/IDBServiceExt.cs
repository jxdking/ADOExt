using System.Data;

namespace MagicEastern.ADOExt
{
    public static class IDBServiceExt
    {
        public static DBConnectionWrapper OpenConnection(this IDBService dbService)
        {
            var conn = dbService.DBClassResolver.CreateConnection();
            conn.Open();
            return new DBConnectionWrapper(conn, dbService);
        }

        public static DBTransactionWrapper BeginTransaction(this IDBService dbService)
        {
            var conn = OpenConnection(dbService);
            return conn.BeginTransaction();
        }

        public static DBTransactionWrapper BeginTransaction(this IDBService dbService, IsolationLevel il)
        {
            var conn = OpenConnection(dbService);
            return conn.BeginTransaction(il);
        }
    }
}
