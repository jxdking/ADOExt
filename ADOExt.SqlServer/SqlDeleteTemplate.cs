using System.Linq;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlDeleteTemplate<T> : SqlDeleteTemplateBase<T>
    {
        public SqlDeleteTemplate(DBTableAdapterContext<T> context, SqlResolver sqlResolver) : base(context)
        {
            var tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            Template = "delete from " + tablename + " where " + string.Join(" and ", PkCols.Select(i => "[" + i + "]=@" + i));
        }
    }
}
