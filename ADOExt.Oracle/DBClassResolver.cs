using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace MagicEastern.ADOExt.Oracle
{
    internal class DBClassResolver : IDBClassResolver
    {
        private string connStr;

        public string DataBaseType => ADOExt.DataBaseType.Oracle;

        public DBClassResolver(string connectionString)
        {
            connStr = connectionString;
        }

        public IDbCommand CreateCommand(Sql sql, IDbConnection conn, IDbTransaction trans = null)
        {
            OracleCommand command = new OracleCommand(sql.Text, (OracleConnection)conn);
            command.Transaction = (OracleTransaction)trans;
            if (sql.Parameters.Count > 0)
            {
                command.BindByName = true;
                command.Parameters.AddRange(sql.Parameters.Select(i => ToOracleParameter(i)).ToArray());
            }
            if (sql.CommandTimeout >= 0)
            {
                command.CommandTimeout = sql.CommandTimeout;
            }
            return command;
        }

        private OracleParameter ToOracleParameter(Parameter parameter)
        {
            var p = new OracleParameter(parameter.Name, parameter.Value ?? DBNull.Value);
            p.Direction = parameter.Direction;
            p.Size = int.MaxValue; // remove the size limitation of the parameter.
            return p;
        }

        public DBErrorType GetDBErrorType(DbException ex)
        {
            return ((OracleException)ex).GetDBErrorType();
        }

        public IDbConnection CreateConnection()
        {
            return new OracleConnection(connStr);
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new OracleDataAdapter((OracleCommand)command);
        }
    }
}