using System;
using System.Data;

namespace MagicEastern.ADOExt
{
    public class DBConnectionWrapper : IDisposable
    {
        public IDbConnection Connection { get; private set; }
        public IDBService DBService { get; }
        private readonly Func<IDbCommand> commandFactory;

        public DBConnectionWrapper(IDbConnection connection, IDBService dbService, Func<IDbCommand> commandFactory)
        {
            Connection = connection;
            DBService = dbService;
            this.commandFactory = commandFactory;
        }

        public IDbCommand CreateCommand() => commandFactory();
        
        public DBTransactionWrapper BeginTransaction()
        {
            var trans = Connection.BeginTransaction();
            return new DBTransactionWrapper(trans, this);
        }

        public DBTransactionWrapper BeginTransaction(IsolationLevel il)
        {
            var trans = Connection.BeginTransaction(il);
            return new DBTransactionWrapper(trans, this);
        }

        public void Close()
        {
            Connection.Close();
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
