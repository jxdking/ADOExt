namespace MagicEastern.ADOExt
{
    public interface IDBTableMappingFactory
    {
        DBTableMapping<T> Get<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans);
    }
}