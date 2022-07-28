namespace MagicEastern.ADOExt
{
    public class DBService : IDBService
    {
        public IDBClassResolver DBClassResolver { get; }
        public ISqlResolver SqlResolver { get; }
        public IDBObjectMappingFactory DBObjectMappingFactory { get; }
        public IDBTableMappingFactory DBTableMappingFactory { get; }
        public IDBTableAdapterFactory DBTableAdapterFactory { get; }

        public DBService(IDBClassResolver dbClassResolver, ISqlResolver sqlResolver
            , IDBObjectMappingFactory dbObjectMappingFactory, IDBTableMappingFactory dbTableMappingFactory
            , IDBTableAdapterFactory dbTableAdapterFactory)
        {
            DBClassResolver = dbClassResolver;
            SqlResolver = sqlResolver;
            DBObjectMappingFactory = dbObjectMappingFactory;
            DBTableMappingFactory = dbTableMappingFactory;
            DBTableAdapterFactory = dbTableAdapterFactory;
        }
    }
}