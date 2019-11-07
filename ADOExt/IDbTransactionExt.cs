using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace MagicEastern.ADOExt
{
    public static class IDbTransactionExt
    {
        public static List<T> Query<T>(this IDbTransaction trans, Sql sql) where T : new()
        {
            return trans.Connection.Query<T>(sql, trans);
        }

        public static T GetSingleValue<T>(this IDbTransaction trans, Sql sql)
        {
            return trans.Connection.GetSingleValue<T>(sql, trans);
        }

        public static List<T> GetFirstColumn<T>(this IDbTransaction trans, Sql sql)
        {
            return trans.Connection.GetFirstColumn<T>(sql, trans);
        }

        public static int Execute(this IDbTransaction trans, Sql sql, bool storeProcedure = false)
        {
            return trans.Connection.Execute(sql, storeProcedure, trans);
        }

        public static T Load<T>(this IDbTransaction trans, T obj) where T : new()
        {
            return trans.Connection.Load<T>(obj, trans);
        }

        public static int Delete<T>(this IDbTransaction trans, T obj) where T : new()
        {
            return trans.Connection.Delete<T>(obj, trans);
        }

        public static int Insert<T>(this IDbTransaction trans, ref T obj) where T : new()
        {
            return trans.Connection.Insert<T>(ref obj, trans);
        }

        public static int Update<T>(this IDbTransaction trans, ref T obj, params Expression<Func<T, object>>[] targetProperties) where T : new()
        {
            return trans.Connection.Update<T>(ref obj, trans, targetProperties);
        }
    }
}