using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt
{
    public static class DBTransactionExt
    {
        public static DataTable Query(this DBTransactionWrapper trans, Sql sql)
        {
            return trans.Connection.Query(sql, trans);
        }

        public static IEnumerable<T> Query<T>(this DBTransactionWrapper trans, Sql sql) where T : new()
        {
            return trans.Connection.Query<T>(sql, trans);
        }

        public static T GetSingleValue<T>(this DBTransactionWrapper trans, Sql sql)
        {
            return trans.Connection.GetSingleValue<T>(sql, trans);
        }

        public static IEnumerable<T> GetFirstColumn<T>(this DBTransactionWrapper trans, Sql sql)
        {
            return trans.Connection.GetFirstColumn<T>(sql, trans);
        }

        public static int Execute(this DBTransactionWrapper trans, Sql sql, bool storeProcedure = false)
        {
            return trans.Connection.Execute(sql, out _, storeProcedure, trans);
        }

        public static int Execute(this DBTransactionWrapper trans, Sql sql, out IEnumerable<Parameter> outputParameters
            , bool storeProcedure = false)
        {
            return trans.Connection.Execute(sql, out outputParameters, storeProcedure, trans);
        }

        public static T Load<T>(this DBTransactionWrapper trans, T obj) where T : new()
        {
            return trans.Connection.Load<T>(obj, trans);
        }

        public static int Delete<T>(this DBTransactionWrapper trans, T obj) where T : new()
        {
            return trans.Connection.Delete<T>(obj, trans);
        }

        public static int Insert<T>(this DBTransactionWrapper trans, T obj, object properties, out T result) where T : new()
        {
            return trans.Connection.Insert<T>(obj, properties, out result, trans);
        }

        public static int Insert<T>(this DBTransactionWrapper trans, T obj, object properties = null) where T : new()
        {
            return trans.Connection.Insert<T>(obj, properties, trans);
        }

        public static int Update<T>(this DBTransactionWrapper trans, T obj, object properties, out T result) where T : new()
        {
            return trans.Connection.Update<T>(obj, properties, out result, trans);
        }

        public static int Update<T>(this DBTransactionWrapper trans, T obj, object properties = null) where T : new()
        {
            return trans.Connection.Update<T>(obj, properties, trans);

        }
    }
}