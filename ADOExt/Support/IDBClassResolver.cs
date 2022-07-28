using System.Data;
using System.Data.Common;

namespace MagicEastern.ADOExt
{
    public interface IDBClassResolver
    {
        IDbConnection CreateConnection();
        IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans);
        IDbDataAdapter CreateDataAdapter(IDbCommand command);
        DBErrorType GetDBErrorType(DbException ex);
    }
}