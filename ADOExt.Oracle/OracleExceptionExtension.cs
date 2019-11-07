using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace MagicEastern.ADOExt.Oracle
{
    using MagicEastern.ADOExt;

    public static class OracleExceptionExtension
    {
        public static Dictionary<int, DBErrorType> DBErrorMapping = new Dictionary<int, DBErrorType> {
            { 1013, DBErrorType.TIMEOUT },
            { 1,  DBErrorType.UNIQUE_VIOLATION },
            { 1422, DBErrorType.TOO_MANY_ROWS },
            { 1403, DBErrorType.NO_DATA_FOUND }
        };

        public static DBErrorType GetDBErrorType(this OracleException ex)
        {
            DBErrorMapping.TryGetValue(ex.Number, out DBErrorType ret);
            return ret;
        }
    }
}