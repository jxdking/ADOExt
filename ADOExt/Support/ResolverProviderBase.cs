using System;
using System.Collections.Specialized;
using System.Data;
using System.Threading;

namespace MagicEastern.ADOExt
{
    public abstract class ResolverProviderBase : IResolverProvider
    {
        public IDBClassResolver DBClassResolver { get; }
        public ISqlResolver SqlResolver { get; }
        /// <summary>
        /// The connection string value after the connection opens.
        /// </summary>
        public string ConnectionString { get; }

        private static int _NextIdx = 0;
        public int Idx { get; }

        public ResolverProviderBase(IDBClassResolver dbClassResolver, ISqlResolver sqlResolver)
        {
            DBClassResolver = dbClassResolver;
            SqlResolver = sqlResolver;
            Idx = Interlocked.Increment(ref _NextIdx) - 1;
            using (var conn = DBClassResolver.CreateConnection()) {
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