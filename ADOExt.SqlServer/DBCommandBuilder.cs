using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace MagicEastern.ADOExt.SqlServer
{
    public class DBCommandBuilder : IDBCommandBuilder
    {
        public IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            SqlCommand command = new SqlCommand(sql.Text, (SqlConnection)conn.Connection);
            if (trans != null)
            {
                if (trans.Transaction == null)
                {
                    throw new InvalidOperationException("The transaction has been committed or rollbacked. No farther operation is allowed.");
                }
                command.Transaction = (SqlTransaction)trans.Transaction;
            }
            foreach (var p in sql.Parameters)
            {
                command.Parameters.Add(ToSqlParameter(p));
            }
            if (sql.CommandTimeout >= 0)
            {
                command.CommandTimeout = sql.CommandTimeout;
            }
            return command;
        }

        private object ToSqlParameter(Parameter parameter)
        {
            SqlParameter p;
            if (parameter.Value == null)
            {
                p = new SqlParameter(parameter.Name, DBNull.Value);
                p.DbType = parameter.DbType;
            }
            else
            {
                p = new SqlParameter(parameter.Name, parameter.Value);
            }
            p.Direction = parameter.Direction;
            if (p.Direction != ParameterDirection.Input)
            {
                p.Size = short.MaxValue; // remove the size limitation of the parameter.
            }
            return p;
        }
    }
}