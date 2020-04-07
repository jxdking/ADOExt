using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MagicEastern.ADOExt
{
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
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
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
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
