namespace MagicEastern.ADOExt
{
    public interface IResolverProvider
    {
        int Idx { get; }

        /// <summary>
        /// The full connection string, which should include user and password.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// The password of the connection string will be stripped away after the DBConnection is opened. The stripped connection string will be saved here.
        /// </summary>
        string ConnectionStringOpened { get; }
        IDBClassResolver DBClassResolver { get; }
        ISqlResolver SqlResolver { get; }

        DBTableAdapter<T> GetDBTableAdapter<T>() where T : new();
    }
}