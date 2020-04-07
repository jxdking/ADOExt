using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IResolverProvider
    {
        int Idx { get; }
        string ConnectionString { get; }
        IDBClassResolver DBClassResolver { get; }
        ISqlResolver SqlResolver { get; }

        IDBTableAdapter<T> GetDBTableAdapter<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) where T : new();

        DBConnectionWrapper OpenConnection();
    }
}