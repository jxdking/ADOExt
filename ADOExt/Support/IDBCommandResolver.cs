using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBCommandResolver
    {
        string DataBaseType { get; }

        IDbCommand CreateCommand(Sql sql, IDbConnection conn, IDbTransaction trans = null);
    }
}