using MagicEastern.ADOExt.Common.OracleLite;
using Oracle.ManagedDataAccess.Client;

namespace MagicEastern.ADOExt.Oracle.Core.Lite
{
    public class SqlUpdateTemplate<T> : SqlUpdateTemplateCommon<T, OracleParameter>
        where T : new()
    {
        public SqlUpdateTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
