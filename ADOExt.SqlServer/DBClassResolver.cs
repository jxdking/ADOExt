using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace MagicEastern.ADOExt.SqlServer
{
    public class DBClassResolver : IDBClassResolver
    {
        public IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            SqlCommand command = new SqlCommand(sql.Text, (SqlConnection)conn.Connection);
            if (trans != null)
            {
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

        public DBErrorType GetDBErrorType(DbException ex)
        {
            return DBErrorType.UNDEFINED;
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
                p.Size = short.MaxValue;
            }
            return p;
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }
    }
}