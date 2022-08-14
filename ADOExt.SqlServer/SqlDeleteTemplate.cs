using MagicEastern.ADOExt.Common.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlDeleteTemplate<T> : SqlDeleteTemplateCommon<T, SqlParameter>
    {
        public SqlDeleteTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
