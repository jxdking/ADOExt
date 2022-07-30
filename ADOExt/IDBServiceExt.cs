using System.Data;

namespace MagicEastern.ADOExt
{
    public static class IDBServiceExt
    {
        public static DBTransactionWrapper BeginTransaction(this IDBService dbService)
        {
            var conn = dbService.OpenConnection();
            return conn.BeginTransaction();
        }

        public static DBTransactionWrapper BeginTransaction(this IDBService dbService, IsolationLevel il)
        {
            var conn = dbService.OpenConnection();
            return conn.BeginTransaction(il);
        }
    }
}
