using System;
using System.Collections.Specialized;
using System.Data;
using System.Threading;

namespace MagicEastern.ADOExt
{
    public abstract class ResolverProviderBase : IResolverProvider
    {
        public abstract IDBClassResolver DBClassResolver { get; }
        public abstract ISqlResolver SqlResolver { get; }
        public string ConnectionString { get; }

        private static int _NextIdx = 0;
        public int Idx { get; }

        public ResolverProviderBase(Func<IDbConnection> createConnection)
        {
            Idx = Interlocked.Increment(ref _NextIdx) - 1;
            using (var conn = createConnection()) {
                conn.Open();
                ConnectionString = conn.ConnectionString;
            }
        }

        public abstract IDBTableAdapter<T> GetDBTableAdapter<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) where T : new();

        public DBConnectionWrapper OpenConnection()
        {
            var conn = DBClassResolver.CreateConnection();
            conn.Open();
            return new DBConnectionWrapper(conn, this);
        }
    }
}