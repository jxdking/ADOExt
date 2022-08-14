using MagicEastern.ADOExt.Common.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlResolver : SqlResolverCommon<SqlParameter>
    {
        public SqlResolver(IServiceProvider sp) : base(sp)
        {
        }
    }
}
