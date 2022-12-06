using MagicEastern.ADOExt.Common.Oracle;
using Oracle.ManagedDataAccess.Client;

namespace MagicEastern.ADOExt.Oracle.Core
{
    public class SqlDeleteTemplate<T> : SqlDeleteTemplateCommon<T, OracleParameter>
    {
        public SqlDeleteTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
