using MagicEastern.ADOExt.Common.Oracle;
using Oracle.ManagedDataAccess.Client;

namespace MagicEastern.ADOExt.Oracle.Core
{
    public class SqlLoadTemplate<T> : SqlLoadTemplateCommon<T, OracleParameter>
    {
        public SqlLoadTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
