using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.Oracle
{
    public class SqlDeleteTemplateCommon<T, TParameter> : SqlDeleteTemplateBase<T, TParameter>
        where TParameter : IDbDataParameter, new()
    {
        public SqlDeleteTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) :
            base(context.PkColumnsInfo, GetTemplateString(context, sqlResolver), sqlResolver)
        {
        }

        private static string GetTemplateString(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            var template = "delete from " + tablename + " where " + string.Join(" and ", context.PkColumnsInfo.Select(i => i.ColumnName + "=:" + i.ColumnName));
            return template;
        }
    }
}
