namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapterFactory
    {
        ICommandBuilderFactory CommandBuilderFactory { get; }

        DBTableAdapter<T> Get<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) where T : new();
    }
}