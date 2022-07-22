using System;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class NoQueryCommand<T> : CommandBase<T> where T : new()
    {
        public NoQueryCommand(string commandText, IEnumerable<CommandParameter<T>> parameters) : base(commandText, parameters)
        {
        }

        public override int Execute(T obj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null)
        {
            var sql = CreateSql(obj);
            int ret = conn.Execute(sql, false, trans);
            result = default(T);
            return ret;
        }
    }
}
