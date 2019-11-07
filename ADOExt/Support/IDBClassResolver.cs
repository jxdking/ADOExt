using System.Data;
using System.Data.Common;

namespace MagicEastern.ADOExt
{
    public interface IDBClassResolver : IDBCommandResolver, IDBConnectionResolver
    {
        IDbDataAdapter CreateDataAdapter(IDbCommand command);

        DBErrorType GetDBErrorType(DbException ex);
    }
}