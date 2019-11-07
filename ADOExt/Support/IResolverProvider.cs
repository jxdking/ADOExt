namespace MagicEastern.ADOExt
{
    public interface IResolverProvider
    {
        int Idx { get; }
        string ConnectionString { get; }
        string ConnectionStringOpened { get; }
        IDBClassResolver DBClassResolver { get; }
        ISqlResolver SqlResolver { get; }

        DBTableAdapter<T> GetDBTableAdapter<T>() where T : new();
    }
}