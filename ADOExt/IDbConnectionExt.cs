using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace MagicEastern.ADOExt
{
    public static class IDbConnectionExt
    {
        public static IDbCommand CreateCommand(this IDbConnection conn, Sql sql, IDbTransaction trans = null)
        {
            var rp = ResolverProviderBase.Get(conn.ConnectionString);
            return rp.DBClassResolver.CreateCommand(sql, conn, trans);
        }

        private static void SetProperty<T>(T obj, IDataRecord reader, IDBColumnMapping<T> mapping, Action<T, IDataRecord, int> setter, int ordinal)
        {
            try
            {
                setter(obj, reader, ordinal);
            }
            catch (Exception ex)
            {
                string colName = mapping.ColumnName;
                throw new FormatException("Failed to Parse [" + typeof(T).Name + "." + colName + "] from DataReader!", ex);
            }
        }

        public static List<T> Query<T>(this IDbConnection conn, Sql sql, IDbTransaction trans = null) where T : new()
        {
            IDbCommand command = conn.CreateCommand(sql, trans);

            List<T> res = new List<T>();

            try
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    var mapping = DBObjectMapping<T>.Get().ColumnMappingList;
                    int[] ordinalAry = new int[mapping.Count];
                    var setterAry = new Action<T, IDataRecord, int>[mapping.Count];

                    T obj;

                    if (reader.Read())
                    {
                        obj = new T();
                        string cName = null;
                        try
                        {
                            for (int i = 0; i < mapping.Count; i++)
                            {
                                cName = mapping[i].ColumnName;
                                ordinalAry[i] = reader.GetOrdinal(cName);
                                setterAry[i] = mapping[i].GetPropSetterForRecord(reader.GetFieldType(ordinalAry[i]));

                                SetProperty(obj, reader, mapping[i], setterAry[i], ordinalAry[i]);
                            }

                            res.Add(obj);
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                            throw new IndexOutOfRangeException("Unable to find specified column[" + cName + "] in DataReader", ex);
                        }
                    }

                    while (reader.Read())
                    {
                        obj = new T();
                        for (int i = 0; i < mapping.Count; i++)
                        {
                            SetProperty(obj, reader, mapping[i], setterAry[i], ordinalAry[i]);
                        }
                        res.Add(obj);
                    }
                }
            }
            catch (DbException ex)
            {
                throw;
                //throw new Exception("Error occurred when running SQL!", CreateBackEndErrorMassage(ex, sql), ex);
            }
            return res;
        }

        public static T GetSingleValue<T>(this IDbConnection conn, Sql sql, IDbTransaction trans = null)
        {
            IDbCommand command = conn.CreateCommand(sql, trans);

            try
            {
                object val = command.ExecuteScalar();
                return ParseFromDBValue<T>(val);
            }
            catch (DbException ex)
            {
                throw;
                //throw new WebAppException("Error occurred when running SQL!", CreateBackEndErrorMassage(ex, sql), ex);
            }
        }

        private class SingleColumn<T>
        {
            [DBColumn]
            public T Val { get; set; }
        }

        public static List<T> GetFirstColumn<T>(this IDbConnection conn, Sql sql, IDbTransaction trans = null)
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
                        SetProperty(obj, reader, mapping, setter, 0);
                        ret.Add(obj.Val);
                    }

                    while (reader.Read())
                    {
                        SetProperty(obj, reader, mapping, setter, 0);
                        ret.Add(obj.Val);
                    }
                }
            }
            catch (DbException ex)
            {
                throw;
                //throw new WebAppException("Error occurred when running SQL!", CreateBackEndErrorMassage(ex, sql), ex);
            }
            return ret;
        }

        public static int Execute(this IDbConnection conn, Sql sql, bool storeProcedure = false, IDbTransaction trans = null)
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
                throw;
                //throw new WebAppException("Error occurred when running SQL!", CreateBackEndErrorMassage(ex, sql), ex);
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
                    //throw new WebAppException("Error occurred when parsing SQL result!", "Cannot assign null to " + typeof(T).FullName + ".");
                }
                return ret;
            }
            var t = typeof(T).UnwrapIfNullable();
            return (T)Convert.ChangeType(obj, t);
        }

        public static T Load<T>(this IDbConnection conn, T obj, IDbTransaction trans = null) where T : new()
        {
            var rp = ResolverProviderBase.Get(conn.ConnectionString);
            var ta = rp.GetDBTableAdapter<T>();
            return ta.Load(obj, conn, trans);
        }

        public static int Delete<T>(this IDbConnection conn, T obj, IDbTransaction trans = null) where T : new()
        {
            var rp = ResolverProviderBase.Get(conn.ConnectionString);
            var ta = rp.GetDBTableAdapter<T>();
            return ta.Delete(obj, conn, trans);
        }

        public static int Insert<T>(this IDbConnection conn, ref T obj, IDbTransaction trans = null) where T : new()
        {
            var rp = ResolverProviderBase.Get(conn.ConnectionString);
            var ta = rp.GetDBTableAdapter<T>();
            return ta.Insert(ref obj, conn, trans);
        }

        public static int Update<T>(this IDbConnection conn, ref T obj, IDbTransaction trans = null, params Expression<Func<T, object>>[] targetProperties) where T : new()
        {
            var rp = ResolverProviderBase.Get(conn.ConnectionString);
            var ta = rp.GetDBTableAdapter<T>();
            return ta.Update(ref obj, conn, trans, targetProperties);
        }
    }
}