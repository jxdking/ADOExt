namespace MagicEastern.ADOExt
{
    public interface ISqlLoadTemplate<T>
    {
        Sql Generate(T obj);
    }
}