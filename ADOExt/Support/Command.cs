using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    internal class Command<T> where T : new()
    {
        public string CommandText;
        public List<IDBColumnMapping<T>> ColumnInfoList;

        public int Execute(ref T obj, IDbConnection conn, IDbTransaction trans = null)
        {
            var sql = CreateSql(obj);
            int ret = conn.Execute(sql, false, trans);
            for (int i = 0; i < ColumnInfoList.Count; i++)
            {
                ColumnInfoList[i].PropertySetter(obj, sql.Parameters[i].Output);
            }
            return ret;
        }

        public T Load(T obj, IDbConnection conn, IDbTransaction trans = null)
        {
            var sql = CreateSql(obj);
            var ret = conn.Query<T>(sql, trans).SingleOrDefault();
            return ret;
        }

        private Sql CreateSql(T obj)
        {
            return new Sql(CommandText, ColumnInfoList.Select(i => CreateParameter(i, obj)));
        }

        private static Parameter CreateParameter(IDBColumnMapping<T> colInfo, T obj)
        {
            return new Parameter(colInfo.ColumnName, colInfo.PropertyGetter(obj), System.Data.ParameterDirection.InputOutput);
        }
    }
}