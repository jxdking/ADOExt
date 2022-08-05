using System.Data;
using System.Data.Common;

namespace MagicEastern.ADOExt
{
    public interface IDBCommandBuilder
    {
        IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans);

        IDbDataParameter CreateParameter();
    }
}