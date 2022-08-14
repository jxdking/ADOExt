using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.SqlServer
{
    public class SqlLoadTemplateCommon<T, TParameter> : SqlLoadTemplateBase<T, TParameter>
        where TParameter : IDbDataParameter, new()
    {
        private SqlLoadTemplateCommon(IEnumerable<IDBColumnMapping<T>> pkCols, string template)
          : base(pkCols, template)
        {
        }

        public SqlLoadTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) :
            this(context.PkColumnsInfo, GetTemplateString(context, sqlResolver))
        {
        }

        private static string GetTemplateString(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            var template = "select [" + string.Join("],[", context.AllColumnsInfo) + "] from " + tablename + " where " + string.Join(" and ", context.PkColumnsInfo.Select(i => "[" + i.ColumnName + "]=@" + i.ColumnName));
            return template;
        }

    }
}
