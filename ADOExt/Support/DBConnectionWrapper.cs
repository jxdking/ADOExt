using System.Data;

namespace MagicEastern.ADOExt
{
    public class DBConnectionWrapper : IDbConnection
    {
        public IDbConnection Connection { get; private set; }
        public IDBService DBService { get; }

        public DBConnectionWrapper(IDbConnection connection, IDBService dbService)
        {
            Connection = connection;
            DBService = dbService;
        }

        public string ConnectionString { get { return Connection.ConnectionString; } set { Connection.ConnectionString = value; } }

        public int ConnectionTimeout => Connection.ConnectionTimeout;

        public string Database => Connection.Database;

        public ConnectionState State => Connection.State;

        public DBTransactionWrapper BeginTransaction()
        {
            var trans = Connection.BeginTransaction();
            return new DBTransactionWrapper(trans, this);
        }

        IDbTransaction IDbConnection.BeginTransaction() => BeginTransaction();

        public DBTransactionWrapper BeginTransaction(IsolationLevel il)
        {
            var trans = Connection.BeginTransaction(il);
            return new DBTransactionWrapper(trans, this);
        }

        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il) => BeginTransaction(il);

        public void ChangeDatabase(string databaseName)
        {
            Connection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            Connection.Close();
        }

        public IDbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        public void Open()
        {
            Connection.Open();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Connection.Dispose();
                    Connection = null;
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

        }
        #endregion
    }
}
