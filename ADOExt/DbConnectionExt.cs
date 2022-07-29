﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace MagicEastern.ADOExt
{
    public static class DBConnectionExt
    {
        public static IDbCommand CreateCommand(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            if (trans != null && trans.Transaction == null) {
                throw new InvalidOperationException("The transaction has been committed or rollbacked. No farther operation is allowed.");
            }
            return  conn.DBService.DBClassResolver.CreateCommand(sql, conn, trans);
        }

        public static DataTable Query(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            IDbCommand command = conn.CreateCommand(sql, trans);
            IDbDataAdapter da = conn.DBService.DBClassResolver.CreateDataAdapter(command);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds.Tables[0];
        }

        public static IEnumerable<T> Query<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null) where T : new()
        {
            var objectMapping = conn.DBService.DBObjectMappingFactory.Get<T>();

            IDbCommand command = conn.CreateCommand(sql, trans);
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
            IDbCommand command = conn.CreateCommand(sql, trans);

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
            IDbCommand command = conn.CreateCommand(sql, trans);

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

        private static IEnumerable<Parameter> GetOutputParameters(IDataParameterCollection paras)
        {
            foreach (var pObj in paras)
            {
                var p = (IDataParameter)pObj;
                if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue)
                {
                    yield return new Parameter
                    {
                        Name = p.ParameterName,
                        Value = p.Value,
                        Direction = p.Direction,
                        DbType = p.DbType
                    };
                }
            }
        }

        public static int Execute(this DBConnectionWrapper conn, Sql sql, bool storeProcedure = false, DBTransactionWrapper trans = null)
        {
            return Execute(conn, sql, out _, storeProcedure, trans);
        }

        public static int Execute(this DBConnectionWrapper conn, Sql sql, out IEnumerable<Parameter> outputParameters
            , bool storeProcedure = false, DBTransactionWrapper trans = null)
        {
            IDbCommand command = conn.CreateCommand(sql, trans);
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
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Load(obj, conn, trans);
        }

        public static int Delete<T>(this DBConnectionWrapper conn, T obj, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Delete(obj, conn, trans);
        }

        public static int Insert<T>(this DBConnectionWrapper conn, T obj, object properties, out T result, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Insert(obj, properties, out result, conn, trans);
        }

        public static int Insert<T>(this DBConnectionWrapper conn, T obj, object properties = null, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Insert(obj, properties, out _, conn, trans);
        }

        public static int Update<T>(this DBConnectionWrapper conn, T obj, object properties, out T result, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Update(obj, properties, out result, conn, trans);
        }

        public static int Update<T>(this DBConnectionWrapper conn, T obj, object properties = null, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Update(obj, properties, out _, conn, trans);
        }
    }

}
