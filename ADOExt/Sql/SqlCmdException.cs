using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class SqlCmdException: DbException
    {
        public Sql Sql { get; }

        public SqlCmdException(string message, Sql sql, Exception innerException) : base(message, innerException)
        {
            Sql = sql;
        }
    }
}
