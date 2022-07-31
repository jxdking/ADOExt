using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;

namespace MagicEastern.ADOExt.Oracle
{
    public class DBClassResolver : IDBClassResolver
    {
        public IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            OracleCommand command = new OracleCommand(sql.Text, (OracleConnection)conn.Connection);
            if (trans != null)
            {
                if (trans.Transaction == null)
                {
                    throw new InvalidOperationException("The transaction has been committed or rollbacked. No farther operation is allowed.");
                }
                command.Transaction = (OracleTransaction)trans.Transaction;
            }
            if (sql.Parameters.Count > 0)
            {
                command.BindByName = true;
                foreach (var p in sql.Parameters)
                {
                    command.Parameters.Add(ToOracleParameter(p));
                }
            }
            if (sql.CommandTimeout >= 0)
            {
                command.CommandTimeout = sql.CommandTimeout;
            }
            return command;
        }

        private OracleParameter ToOracleParameter(Parameter parameter)
        {
            OracleParameter p;
            if (parameter.Value == null)
            {
                p = new OracleParameter(parameter.Name, DBNull.Value);
                p.DbType = parameter.DbType;
            }
            else
            {
                p = new OracleParameter(parameter.Name, parameter.Value);
            }
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

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new OracleDataAdapter((OracleCommand)command);
        }
    }
}