using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBConnectionResolver
    {
        IDbConnection CreateConnection();
    }
}