namespace MagicEastern.ADOExt
{
    public interface IDBColumnMappingInfo<T> : IDBColumnMapping<T>
    {
        string TableName { get; }
        string DataType { get; }
        int? CharLength { get; }
        int? DataPrecision { get; }
        int? DataScale { get; }
        bool PK { get; }
    }
}