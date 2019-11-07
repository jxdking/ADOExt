using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBConnectionResolver
    {
        string DataBaseType { get; }

        IDbConnection CreateConnection();
    }
}