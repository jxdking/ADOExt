namespace MagicEastern.ADOExt
{
    public interface IDBObjectMappingFactory
    {
        DBObjectMapping<T> Get<T>();
    }
}