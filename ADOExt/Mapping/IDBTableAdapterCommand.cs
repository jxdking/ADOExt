using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBTableAdapterCommand<T> where T : new()
    {
        int Execute(ref T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null);
    }
}