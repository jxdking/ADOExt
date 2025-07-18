using System;
using System.Data;

namespace MagicEastern.ADOExt
{
    public class DBConnectionWrapper : IDisposable
    {
        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction { get; private set; }
        public IDBService DBService { get; }
        private readonly Func<IDbCommand> commandFactory;

        public void Commit() {
            if (Transaction == null)
            {
                throw new InvalidOperationException("The transaction has not started yet or it has completed already.");
            }
            Transaction.Commit();
            Dispose(true);
        }
        public void Rollback() {
            if (Transaction == null)
            {
                throw new InvalidOperationException("The transaction has not started yet or it has completed already.");
            }
            Transaction.Rollback();
            Dispose(true);
        }

        public DBConnectionWrapper(IDbConnection connection, IDBService dbService, Func<IDbCommand> commandFactory, IDbTransaction transaction = null)
        {
            Connection = connection;
            DBService = dbService;
            this.commandFactory = commandFactory;
            Transaction = transaction;
        }

        public IDbCommand CreateCommand() => commandFactory();
        
        public void BeginTransaction()
        {
            Transaction = Connection.BeginTransaction();
        }

        public void BeginTransaction(IsolationLevel il)
        {
            Transaction = Connection.BeginTransaction(il);
        }

        public void Close()
        {
            Connection?.Close();
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
                    Transaction?.Dispose();
                    Transaction = null;
                    Connection?.Dispose();
                    Connection = null;
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
            }

            Dispose(true);
        }
        #endregion
    }
}
