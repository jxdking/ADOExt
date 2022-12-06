using MagicEastern.ADOExt.Common.Oracle;
using Oracle.ManagedDataAccess.Client;

namespace MagicEastern.ADOExt.Oracle.Core
{
    public class SqlInsertTemplate<T> : SqlInsertTemplateCommon<T, OracleParameter>
        where T : new()
    {
        public SqlInsertTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
