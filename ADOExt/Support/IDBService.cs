namespace MagicEastern.ADOExt
{
    public interface IDBService
    {
        IDBClassResolver DBClassResolver { get; }
        ISqlResolver SqlResolver { get; }
        IDBObjectMappingFactory DBObjectMappingFactory { get; }
        IDBTableMappingFactory DBTableMappingFactory { get; }
        IDBTableAdapterFactory DBTableAdapterFactory { get; }
    }
}