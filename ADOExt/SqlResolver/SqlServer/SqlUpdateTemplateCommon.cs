using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.SqlServer
{
    public class SqlUpdateTemplateCommon<T, TParameter> : SqlUpdateTemplateBase<T>
        where TParameter : IDbDataParameter, new()
    {
        private readonly DBTableAdapterContext<T> context;
        private readonly ISqlResolver sqlResolver;
        private string Template;

        public SqlUpdateTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            Template = "update " + tablename + " set {0} where " + string.Join(" and ", context.PkColumnsInfo.Select(i => "[" + i.ColumnName + "]=@" + i.ColumnName));
            this.context = context;
            this.sqlResolver = sqlResolver;
        }

        public override int Execute(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            var sql = Generate(obj, setCols);
            result = default(T);
            return conn.Execute(sql, false, trans);
        }

        public override Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols)
        {
            if (!(setCols is List<IDBTableColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }
            var sqltxt = string.Format(Template, string.Join(",", cols.Select(i => string.Join("", new string[] { "[", i.ColumnName, "]=@", i.ColumnName }))));
            return new Sql(sqltxt, cols.Concat(context.PkColumnsInfo).Select(i => (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Input)));
        }
    }
}
