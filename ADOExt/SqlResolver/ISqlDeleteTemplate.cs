namespace MagicEastern.ADOExt
{
    public interface ISqlDeleteTemplate<T>
    {
        Sql Generate(T obj, string parameterSuffix = null);
    }
}