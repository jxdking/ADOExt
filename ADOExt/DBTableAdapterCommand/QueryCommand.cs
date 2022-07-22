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

        public override int Execute(T inputObj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null)
        {
            var sql = CreateSql(inputObj);
            result = conn.Query<T>(sql, trans).FirstOrDefault();
            return result == null ? 0 : 1;
        }
    }
}
