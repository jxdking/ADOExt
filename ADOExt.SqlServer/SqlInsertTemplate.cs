using MagicEastern.ADOExt.Common.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class SqlInsertTemplate<T> : SqlInsertTemplateCommon<T, SqlParameter>
    {
        public SqlInsertTemplate(DBTableAdapterContext<T> context, ISqlResolver sqlResolver) : base(context, sqlResolver)
        {
        }
    }
}
