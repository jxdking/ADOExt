using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace MagicEastern.ADOExt
{
    public static class DBConnectionExt
    {
        public static IDbCommand CreateCommand(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            return conn.DBService.DBClassResolver.CreateCommand(sql, conn, trans);
        }

        public static IEnumerable<T> Query<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null) where T : new()
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

            // reader will be closed inside reader.AsEnumerable();
            return reader.AsEnumerable<T>(conn.DBService.DBObjectMappingFactory);
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

        private class SingleColumn<T>
        {
            [DBColumn]
            public T Val { get; set; }
        }

        public static IEnumerable<T> GetFirstColumn<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            IDbCommand command = conn.CreateCommand(sql, trans);

            var mapping = conn.DBService.DBObjectMappingFactory.Get<SingleColumn<T>>().ColumnMappingList.Single();

            IDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch (DbException ex)
            {
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }

            SingleColumn<T> obj;
            Action<SingleColumn<T>, IDataRecord, int> setter = mapping.GetPropSetterForRecord(reader.GetFieldType(0));

            // reader will be closed inside reader.AsEnumerable();
            return reader.AsEnumerable().Transform(record =>
            {
                obj = new SingleColumn<T>();
                reader.SetProperty(obj, mapping, setter, 0);
                return obj.Val;
            });
        }

        public static int Execute(this DBConnectionWrapper conn, Sql sql, bool storeProcedure = false, DBTransactionWrapper trans = null)
        {
            IDbCommand command = conn.CreateCommand(sql, trans);
            if (storeProcedure) { command.CommandType = CommandType.StoredProcedure; }

            int ret;
            try
            {
                ret = command.ExecuteNonQuery();
                for (int i = 0; i < sql.Parameters.Count; i++)
                {
                    var p = sql.Parameters[i];
                    if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue)
                    {
                        var val = ((DbParameter)command.Parameters[p.Name]).Value;
                        p.Output = (val == DBNull.Value) ? null : val;
                    }
                }
            }
            catch (DbException ex)
            {
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }

            return ret;
        }

        private static T ParseFromDBValue<T>(object obj)
        {
            T ret;
            if (obj == null || Convert.IsDBNull(obj))
            {
                ret = default(T);
                if (ret != null)
                {
                    throw new NoNullAllowedException("Cannot assign null to " + typeof(T).FullName + ".");
                }
                return ret;
            }
            var t = typeof(T).UnwrapIfNullable();
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

        public static int Insert<T>(this DBConnectionWrapper conn, T obj, out T result, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Insert(obj, conn, out result, trans);
        }

        public static int Insert<T>(this DBConnectionWrapper conn, T obj, DBTransactionWrapper trans = null) where T : new()
        {
            return Insert(conn, obj, out _, trans);
        }

        public static int Update<T>(this DBConnectionWrapper conn, T obj, out T result, DBTransactionWrapper trans = null, params Expression<Func<T, object>>[] targetProperties) where T : new()
        {
            var ta = conn.DBService.DBTableAdapterFactory.Get<T>(conn, trans);
            return ta.Update(obj, conn, out result, trans, targetProperties);
        }

        public static int Update<T>(this DBConnectionWrapper conn, T obj, DBTransactionWrapper trans = null, params Expression<Func<T, object>>[] targetProperties) where T : new()
        {
            return Update(conn, obj, out _, trans, targetProperties);
        }
    }

}
