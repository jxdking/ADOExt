using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace MagicEastern.ADOExt
{
    public static class DbConnectionExt
    {
        public static IDbCommand CreateCommand(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            return conn.ResolverProvider.DBClassResolver.CreateCommand(sql, conn, trans);
        }

        public static List<T> Query<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null) where T : new()
        {
            IDbCommand command = conn.CreateCommand(sql, trans);

            try
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    return reader.ToList<T>();
                }
            }
            catch (DbException ex)
            {
                //throw;
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }
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
                //throw;
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }
        }

        private class SingleColumn<T>
        {
            [DBColumn]
            public T Val { get; set; }
        }

        public static List<T> GetFirstColumn<T>(this DBConnectionWrapper conn, Sql sql, DBTransactionWrapper trans = null)
        {
            IDbCommand command = conn.CreateCommand(sql, trans);

            var mapping = DBObjectMapping<SingleColumn<T>>.Get().ColumnMappingList.Single();

            List<T> ret = new List<T>();

            try
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    SingleColumn<T> obj = new SingleColumn<T>();

                    Action<SingleColumn<T>, IDataRecord, int> setter = null;
                    if (reader.Read())
                    {
                        setter = mapping.GetPropSetterForRecord(reader.GetFieldType(0));
                        reader.SetProperty(obj, mapping, setter, 0);
                        ret.Add(obj.Val);
                    }

                    while (reader.Read())
                    {
                        reader.SetProperty(obj, mapping, setter, 0);
                        ret.Add(obj.Val);
                    }
                }
            }
            catch (DbException ex)
            {
                //throw;
                throw new SqlCmdException("Error occurred when running SQL!", sql, ex);
            }
            return ret;
        }

        public static int Execute(this DBConnectionWrapper conn, Sql sql, bool storeProcedure = false, DBTransactionWrapper trans = null)
        {
            IDbCommand command = conn.CreateCommand(sql, trans);
            if (storeProcedure) { command.CommandType = CommandType.StoredProcedure; }

            int ret = 0;
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
                //throw;
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
            var ta = conn.ResolverProvider.GetDBTableAdapter<T>(conn, trans);
            return ta.Load(obj, conn, trans);
        }

        public static int Delete<T>(this DBConnectionWrapper conn, T obj, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.ResolverProvider.GetDBTableAdapter<T>(conn, trans);
            return ta.Delete(obj, conn, trans);
        }

        public static int Insert<T>(this DBConnectionWrapper conn, ref T obj, DBTransactionWrapper trans = null) where T : new()
        {
            var ta = conn.ResolverProvider.GetDBTableAdapter<T>(conn, trans);
            return ta.Insert(ref obj, conn, trans);
        }

        public static int Update<T>(this DBConnectionWrapper conn, ref T obj, DBTransactionWrapper trans = null, params Expression<Func<T, object>>[] targetProperties) where T : new()
        {
            var ta = conn.ResolverProvider.GetDBTableAdapter<T>(conn, trans);
            return ta.Update(ref obj, conn, trans, targetProperties);
        }
    }

}