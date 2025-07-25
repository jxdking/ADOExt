﻿using System.Data;
using System.Runtime.CompilerServices;

namespace MagicEastern.ADOExt
{
    public static class IDBServiceExt
    {
        public static DBConnectionWrapper BeginTransaction(this IDBService dbService)
        {
            var conn = dbService.OpenConnection();
            conn.BeginTransaction();
            return conn;
        }

        public static DBConnectionWrapper BeginTransaction(this IDBService dbService, IsolationLevel il)
        {
            var conn = dbService.OpenConnection();
            conn.BeginTransaction(il);
            return conn;
        }

        public static Sql GetDeleteSql<T>(this IDBService dbService, T obj, string parameterSuffix = null) where T: new() { 
            var sql = dbService.GetDBTableAdapter<T>().GetDeleteSql(obj, parameterSuffix);
            return sql;
        }

        public static Sql GetInsertSql<T>(this IDBService dbService, T obj, object properties = null, string parameterSuffix = null) where T: new()
        {
            var sql = dbService.GetDBTableAdapter<T>().GetInsertSql(obj, properties, parameterSuffix);
            return sql;
        }

        public static Sql GetUpdateSql<T>(this IDBService dbService, T obj, object properties = null, string parameterSuffix = null) where T : new() { 
            var sql = dbService.GetDBTableAdapter<T>().GetUpdateSql(obj, properties, parameterSuffix);
            return sql;
        }
    }
}
