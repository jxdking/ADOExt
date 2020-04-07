using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class QueryCommand<T> : CommandBase<T> where T : new()
    {
        public QueryCommand(string commandText, IEnumerable<CommandParameter<T>> parameters) : base(commandText, parameters)
        {
        }

        public override int Execute(ref T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            var sql = CreateSql(obj);
            var ret = conn.Query<T>(sql, trans).FirstOrDefault();
            if (ret != null)
            {
                obj = ret;
                return 1;
            }
            return 0;
        }
    }
}
