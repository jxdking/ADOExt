namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapterFactory
    {
        DBTableAdapter<T> Get<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) where T : new();
    }
}