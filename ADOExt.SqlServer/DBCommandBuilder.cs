using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace MagicEastern.ADOExt.SqlServer
{
    public class DBCommandBuilder : IDBCommandBuilder
    {
        private void AttachConnection(IDbCommand command, DBConnectionWrapper conn, DBTransactionWrapper trans) {
            command.Connection = conn.Connection;
            if (trans != null)
            {
                if (trans.Transaction == null)
                {
                    throw new InvalidOperationException("The transaction has been committed or rollbacked. No farther operation is allowed.");
                }
                command.Transaction = trans.Transaction;
            }
        }


        public IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            if (sql.Command != null) {
                AttachConnection(sql.Command, conn, trans);
                return sql.Command;
            }
            SqlCommand command = new SqlCommand(sql.Text);
            AttachConnection(command, conn, trans);
            var paras = sql.ParseParameters<SqlParameter>();
            foreach (var p in paras)
            {
                command.Parameters.Add(Scrub(p));
            }
            if (sql.CommandTimeout >= 0)
            {
                command.CommandTimeout = sql.CommandTimeout;
            }
            sql.Command = command;
            return command;
        }

        public IDbDataParameter CreateParameter()
        {
            return new SqlParameter();
        }

        private SqlParameter Scrub(SqlParameter parameter)
        {
            if (parameter.Value == null)
            {
                parameter.Value = DBNull.Value;
            }
            if (parameter.Direction != ParameterDirection.Input)
            {
                parameter.Size = short.MaxValue; // remove the size limitation of the parameter.
            }
            return parameter;
        }
    }
}