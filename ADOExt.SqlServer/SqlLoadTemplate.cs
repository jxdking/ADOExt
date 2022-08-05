using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlLoadTemplate<T> : SqlLoadTemplateBase<T>
    {
        private SqlLoadTemplate(IEnumerable<IDBColumnMapping<T>> pkCols, IDBCommandBuilder commandBuilder, string template)
          : base(pkCols, commandBuilder, template)
        {
        }

        public SqlLoadTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver, IDBCommandBuilder commandBuilder) :
            this(context.PkColumnsInfo, commandBuilder, GetTemplateString(context, sqlResolver))
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
