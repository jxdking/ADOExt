using System.Linq;

namespace MagicEastern.ADOExt.Oracle
{
    public class SqlLoadTemplate<T> : SqlLoadTemplateBase<T>
    {
        public SqlLoadTemplate(DBTableAdapterContext<T> context, SqlResolver sqlResolver) : base(context)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            Template = "select " + string.Join(",", context.AllColumns) + " from " + tablename + " where " + string.Join(" and ", PkCols.Select(i => i + "=:" + i));
        }
    }
}
