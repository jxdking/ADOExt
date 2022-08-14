using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace MagicEastern.ADOExt
{
    public static class DBConnectionWrapperExt
    {
        private static void AttachConnection(IDbCommand command, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
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

        private static IDbCommand CreateCommand(Sql sql, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            if (sql.Command != null)
            {
                AttachConnection(sql.Command, conn, trans);
                return sql.Command;
            }
            var command = conn.CreateCommand();
            command.CommandText = sql.Text;
            AttachConnection(command, conn, trans);
            var paras = sql.ParseParameters(command);
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

        private static IDbDataParameter Scrub(IDbDataParameter parameter)
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

        public static DataTable Query(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            IDbCommand command = CreateCommand(sql, conn, trans);
            using (var reader = command.ExecuteReader())
            {
                var dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }

        public static IEnumerable<T> Query<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null) where T : new()
        {
            var objectMapping = conn.DBService.GetDBObjectMapping<T>();

            IDbCommand command = CreateCommand(sql, conn, trans);
            IDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch (DbException ex)
            {
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }

            var setters = sql.CacheDataReaderSchema
                ? objectMapping.GetDataReaderSetters(sql.Text, reader) : objectMapping.GetDataReaderSetters(reader);

            // reader will be closed inside reader.AsEnumerable();
            return reader.AsEnumerable(setters);
        }

        public static T GetSingleValue<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            IDbCommand command = CreateCommand(sql, conn, trans);

            try
            {
                object val = command.ExecuteScalar();
                return ParseFromDBValue<T>(val);
            }
            catch (DbException ex)
            {
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }
        }

        public static IEnumerable<T> GetFirstColumn<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            IDbCommand command = CreateCommand(sql, conn, trans);

            IDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch (DbException ex)
            {
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }

            return reader.AsEnumerable().Transform(record => ParseFromDBValue<T>(record.GetValue(0)));
        }

        private static IEnumerable<IDataParameter> GetOutputParameters(IDataParameterCollection paras)
        {
            for (int i = 0; i < paras.Count; i++)
            {
                var p = (IDataParameter)paras[i];
                if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue)
                {
                    yield return p;
                }
            }
        }

        public static int Execute(this DBConnectionWrapper conn, Sql sql, bool storeProcedure = false, DBTransactionWrapper trans = null)
        {
            return Execute(conn, sql, out _, storeProcedure, trans);
        }

        public static int Execute(this DBConnectionWrapper conn, Sql sql, out IEnumerable<IDataParameter> outputParameters
            , bool storeProcedure = false, DBTransactionWrapper trans = null)
        {
            IDbCommand command = CreateCommand(sql, conn, trans);
            if (storeProcedure) { command.CommandType = CommandType.StoredProcedure; }

            int ret;
            try
            {
                ret = command.ExecuteNonQuery();
                outputParameters = GetOutputParameters(command.Parameters);
            }
            catch (DbException ex)
            {
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }

            return ret;
        }

        private static T ParseFromDBValue<T>(object obj)
        {
            if (obj == null || DBNull.Value.Equals(obj))
            {
                return (T)(object)null;
            }
            if (obj is T val)
            {
                return val;
            }
            var ut = Nullable.GetUnderlyingType(typeof(T));
            if (ut == obj.GetType()) { return (T)obj; }
            var t = ut ?? typeof(T);
            return (T)Convert.ChangeType(obj, t);
        }

        public static T Load<T>(this DBConnectionWrapper conn, T obj, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.GetDBTableAdapter<T>();
            return ta.Load(obj, conn, trans);
        }

        public static int Delete<T>(this DBConnectionWrapper conn, T obj, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.GetDBTableAdapter<T>();
            return ta.Delete(obj, conn, trans);
        }

        public static int Insert<T>(this DBConnectionWrapper conn, T obj, object properties, out T result, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.GetDBTableAdapter<T>();
            return ta.Insert(obj, properties, out result, conn, trans);
        }

        public static int Insert<T>(this DBConnectionWrapper conn, T obj, object properties = null, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.GetDBTableAdapter<T>();
            return ta.Insert(obj, properties, out _, conn, trans);
        }

        public static int Update<T>(this DBConnectionWrapper conn, T obj, object properties, out T result, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.GetDBTableAdapter<T>();
            return ta.Update(obj, properties, out result, conn, trans);
        }

        public static int Update<T>(this DBConnectionWrapper conn, T obj, object properties = null, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.GetDBTableAdapter<T>();
            return ta.Update(obj, properties, out _, conn, trans);
        }
    }

}
