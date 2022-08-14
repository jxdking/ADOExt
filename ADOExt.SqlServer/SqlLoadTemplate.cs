using MagicEastern.ADOExt.Common.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlLoadTemplate<T> : SqlLoadTemplateCommon<T, SqlParameter>
    {
        public SqlLoadTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
