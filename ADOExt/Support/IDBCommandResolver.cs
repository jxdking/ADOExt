using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBCommandResolver
    {
        IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans = null);
    }
}