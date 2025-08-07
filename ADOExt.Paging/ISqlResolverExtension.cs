using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.Paging
{
    public static class IResolverProviderExt
    {
        public static bool IsOracle(this IDBService db)
        {
            return db.SqlResolver.GetType().FullName == "MagicEastern.ADOExt.Oracle.SqlResolver";
        }

        public static bool IsSqlServer(this IDBService db)
        {
            return db.SqlResolver.GetType().FullName == "MagicEastern.ADOExt.SqlServer.SqlResolver";
        }
    }

    public static class ISqlResolverExtension
    {
        public static string Paging(this IDBService db, string baseSql, int offset, int pagesize)
        {
            if (db.IsOracle())
            {
                return string.Format("select /*+ first_rows({3}) */ * from (select q.*, rownum SQL_GENERATOR__RN from ({0}) q where rownum <= {1}) where SQL_GENERATOR__RN > {2}", baseSql, offset + pagesize, offset, pagesize);
            }
            else if (db.IsSqlServer())
            {
                return string.Format("{0} offset {1} rows fetch next {2} rows only", baseSql, offset, pagesize);
            }
            throw new NotImplementedException();
        }

        public static string GetOrderedSql(this IDBService db, string sqlTxt, IEnumerable<SortPara> sortExprs)
        {
            if (sortExprs.Count() > 0)
            {
                var orderbys = sortExprs.Select(sortExpr => OrderByCondition(sortExpr.PropertyName, sortExpr.IsDesc));
                return string.Format("select * from ({0}) q order by {1}", sqlTxt, string.Join(",", orderbys));
            }


            // "order by" clause is required in some SQL, e.g. "offset" / "fetch next" requires "order by" clause
            return string.Format("select q.* from ({0}) q order by 1", sqlTxt);

        }

        private static string OrderByCondition(string exp, bool isDesc)
        {
            if (int.TryParse(exp, out _))
            {
                return isDesc ? $"{exp} desc" : exp;
            }
            return isDesc ? ($"case when {exp} is null then 1 else 0 end, {exp} desc") : exp;
        }

        public static Sql GetCountSql(this IDBService db, Sql sql)
        {
            Sql countSql = new Sql(string.Format("select count(*) from ({0}) q", sql.Text), sql.Parameters.Values);
            return countSql;
        }
    }
}