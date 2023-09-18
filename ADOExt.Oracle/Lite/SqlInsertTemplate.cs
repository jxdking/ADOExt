using MagicEastern.ADOExt.Common.OracleLite;
using Oracle.ManagedDataAccess.Client;

namespace MagicEastern.ADOExt.Oracle.Lite
{
    public class SqlInsertTemplate<T> : SqlInsertTemplateCommon<T, OracleParameter>
        where T : new()
    {
        public SqlInsertTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
