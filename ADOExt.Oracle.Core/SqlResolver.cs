using MagicEastern.ADOExt.Common.Oracle;
using Oracle.ManagedDataAccess.Client;
using System;

namespace MagicEastern.ADOExt.Oracle.Core
{
    public class SqlResolver : SqlResolverCommon<OracleParameter>
    {
        public SqlResolver(IServiceProvider sp) : base(sp)
        {
        }
    }
}
