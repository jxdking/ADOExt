using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace MagicEastern.ADOExt.Oracle
{
    public class DBClassResolver : IDBClassResolver
    {
        private Func<IDbConnection> _CreateConnection;

        public DBClassResolver(Func<IDbConnection> createConnection)
        {
            _CreateConnection = createConnection;
        }

        public IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            OracleCommand command = new OracleCommand(sql.Text, (OracleConnection)conn.Connection);
            if (trans != null)
            {
                command.Transaction = (OracleTransaction)trans.Transaction;
            }
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
            if (p.Direction != ParameterDirection.Input)
            {
                p.Size = short.MaxValue; // remove the size limitation of the parameter.
            }
            return p;
        }

        public DBErrorType GetDBErrorType(DbException ex)
        {
            return ((OracleException)ex).GetDBErrorType();
        }

        public IDbConnection CreateConnection()
        {
            return _CreateConnection();
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new OracleDataAdapter((OracleCommand)command);
        }
    }
}