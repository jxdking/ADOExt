using System;
using System.Data;

namespace MagicEastern.ADOExt
{
    /// <summary>
    /// The transaction object that rollbacks on Dispose() by default.
    /// </summary>
    public class DBTransactionWrapper : IDbTransaction
    {
        public IDbTransaction Transaction { get; private set; }
        public DBConnectionWrapper Connection { get; private set; }
        IDbConnection IDbTransaction.Connection => Connection;

        public DBTransactionWrapper(IDbTransaction transaction, DBConnectionWrapper connection)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public IsolationLevel IsolationLevel => Transaction.IsolationLevel;

        public void Commit()
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("The transaction has completed already.");
            }
            Transaction.Commit();
            Dispose(true);
        }

        public void Rollback()
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("The transaction has completed already.");
            }
            Transaction.Rollback();
            Dispose(true);
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
                    Transaction.Dispose();
                    Transaction = null;
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // The transaction is never Commit() or Rollback(), do the Rollback() on Dispose() by default.
            if (Transaction != null)
            {
                Transaction.Rollback();
            }

            Dispose(true);
        }

        ~DBTransactionWrapper()
        {
            Dispose(false);
        }
        #endregion

    }
}
