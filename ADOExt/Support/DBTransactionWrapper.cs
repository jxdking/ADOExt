using System;
using System.Data;

namespace MagicEastern.ADOExt
{
    /// <summary>
    /// The transaction object that rollbacks on Dispose() by default.
    /// </summary>
    public class DBTransactionWrapper: IDisposable
    {
        public IDbTransaction Transaction { get; private set; }
        public DBConnectionWrapper Connection { get; private set; }
        
        public DBTransactionWrapper(IDbTransaction transaction, DBConnectionWrapper connection)
        {
            Connection = connection;
            Transaction = transaction;
        }

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
                    Connection?.Dispose();
                    Connection = null;
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // If the transaction is never Commit() or Rollback(), do the Rollback() on Dispose() by default.
            if (Transaction != null)
            {
                Transaction.Rollback();
            }

            Dispose(true);
        }
        #endregion

    }
}
