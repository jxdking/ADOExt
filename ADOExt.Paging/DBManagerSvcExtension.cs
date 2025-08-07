using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace MagicEastern.ADOExt.Paging
{
    public static class DBManagerSvcExtension
    {
        public static DataTable GetPagedDataSet(this DBConnectionWrapper conn, Sql sql, IPagingContext pc)
        {
            Sql countSql = conn.DBService.GetCountSql(sql);
            Sql pagedSql = GetPagedSql(conn.DBService, sql, pc);

            pc.TotalLines = conn.GetSingleValue<int>(countSql);
            var ret = conn.Query(pagedSql);

            ret.Columns.TryRemoveColumn("SQL_GENERATOR__NATURAL_ORDER");
            ret.Columns.TryRemoveColumn("SQL_GENERATOR__RN");
            return ret;
        }

        public static Sql GetPagedSql<T>(this IDBService db, Sql sql, IPagingContext pc) where T : new()
        {
            Sql _sql = new Sql(sql.Text, sql.Parameters.Values);
            var mappingList = db.GetDBObjectMapping<T>().ColumnMappingList;
            string[] cols = mappingList.Select(i => i.ColumnName).ToArray();
            string[] props = mappingList.Select(i => i.ObjectProperty.Name.ToLower()).ToArray();
            IEnumerable<string> whiteList = cols.Union(pc.SortableExpressions).Select(i => i.ToLower());
            IEnumerable<SortPara> exprs = pc.SortParaList.Select(i =>
            {
                int idx = Array.IndexOf(props, i.PropertyName.ToLower());
                if (idx >= 0)
                {
                    // translate property name to column name
                    return new SortPara { PropertyName = cols[idx], IsDesc = i.IsDesc };
                }
                return i;
            });

            var invalid = exprs.Where(i => !whiteList.Contains(i.PropertyName.ToLower()));
            if (invalid.Count() > 0)
            {
                throw new ArgumentOutOfRangeException("Order by [" + invalid.First() + "] is not allowed. ");
            }

            string orderedSql = db.GetOrderedSql(_sql.Text, exprs);
            int offset = (pc.CurrentPage - 1) * pc.PageSize;
            _sql.Text = db.Paging(orderedSql, offset, pc.PageSize);
            return _sql;
        }

        public static IEnumerable<T> GetPagedCollection<T>(this DBConnectionWrapper conn, Sql sql, IPagingContext pc) where T : new()
        {
            Sql countSql = conn.DBService.GetCountSql(sql);
            Sql pagedSql = GetPagedSql<T>(conn.DBService, sql, pc);

            pc.TotalLines = conn.GetSingleValue<int>(countSql);
            var ret = conn.Query<T>(pagedSql);
            return ret;
        }

        public static Sql GetPagedSql(this IDBService db, Sql sql, IPagingContext pc)
        {
            IEnumerable<string> whiteList = pc.SortableExpressions.Select(i => i.ToLower());
            IEnumerable<SortPara> exprs = pc.SortParaList;

            var invalid = exprs.Where(i => !whiteList.Contains(i.PropertyName.ToLower()));
            if (invalid.Count() > 0)
            {
                throw new ArgumentOutOfRangeException("Order by [" + invalid.First() + "] is not allowed. ");
            }

            string orderedSql = db.GetOrderedSql(sql.Text, exprs);
            int offset = (pc.CurrentPage - 1) * pc.PageSize;
            orderedSql = db.Paging(orderedSql, offset, pc.PageSize);
            return new Sql(orderedSql, sql.Parameters.Values);
        }
    }
}
