using MagicEastern.ADOExt.Common.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlUpdateTemplate<T> : SqlUpdateTemplateCommon<T, SqlParameter>
    {
        public SqlUpdateTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
