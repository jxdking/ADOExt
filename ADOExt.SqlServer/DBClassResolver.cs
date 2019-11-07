using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace MagicEastern.ADOExt.SqlServer
{
    internal class DBClassResolver : IDBClassResolver
    {
        private string connString;

        public string DataBaseType => ADOExt.DataBaseType.SqlServer;

        public DBClassResolver(string connectionString)
        {
            connString = connectionString;
        }

        public IDbCommand CreateCommand(Sql sql, IDbConnection conn, IDbTransaction trans = null)
        {
            SqlCommand command = new SqlCommand(sql.Text, (SqlConnection)conn);
            command.Transaction = (SqlTransaction)trans;
            if (sql.Parameters.Count > 0)
            {
                command.Parameters.AddRange(sql.Parameters.Select(i => ToSqlParameter(i)).ToArray());
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
            var p = new SqlParameter(parameter.Name, parameter.Value ?? DBNull.Value);
            p.Direction = parameter.Direction;
            return p;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(connString);
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }
    }
}