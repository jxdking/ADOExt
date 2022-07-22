using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace MagicEastern.ADOExt
{
    public static class DBTransactionExt
    {
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
            return trans.Connection.Execute(sql, storeProcedure, trans);
        }

        public static T Load<T>(this DBTransactionWrapper trans, T obj) where T : new()
        {
            return trans.Connection.Load<T>(obj, trans);
        }

        public static int Delete<T>(this DBTransactionWrapper trans, T obj) where T : new()
        {
            return trans.Connection.Delete<T>(obj, trans);
        }

        public static int Insert<T>(this DBTransactionWrapper trans, T obj, out T result) where T : new()
        {
            return trans.Connection.Insert<T>(obj, out result, trans);
        }

        public static int Insert<T>(this DBTransactionWrapper trans, T obj) where T : new()
        {
            return trans.Connection.Insert<T>(obj, out _, trans);
        }

        public static int Update<T>(this DBTransactionWrapper trans, T obj, out T result, params Expression<Func<T, object>>[] targetProperties) where T : new()
        {
            return trans.Connection.Update<T>(obj, out result, trans, targetProperties);
        }

        public static int Update<T>(this DBTransactionWrapper trans, T obj, params Expression<Func<T, object>>[] targetProperties) where T : new()
        {
            return trans.Connection.Update<T>(obj, out _, trans, targetProperties);
        }
    }
}