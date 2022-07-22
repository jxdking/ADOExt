using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapterCommand<T> where T : new()
    {
        int Execute(T inputObj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null);
    }
}