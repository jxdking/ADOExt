using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace MagicEastern.ADOExt.SqlServer
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
            SqlCommand command = new SqlCommand(sql.Text, (SqlConnection)conn.Connection);
            if (trans != null) {
                command.Transaction = (SqlTransaction)trans.Transaction;
            }
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
            if (p.Direction != ParameterDirection.Input) {
                p.Size = short.MaxValue;
            }
            p.Direction = parameter.Direction;
            return p;
        }

        public IDbConnection CreateConnection()
        {
            return _CreateConnection();
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }
    }
}