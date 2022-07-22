using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEastern.ADOExt.Oracle
{
    public class ReturningCommand<T> : CommandBase<T> where T : new()
    {
        public ReturningCommand(string commandText, IEnumerable<CommandParameter<T>> parameters) : base(commandText, parameters)
        {
        }

        private readonly string RowCountParaName = "sql_nor";

        public override int Execute(T inputObj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null)
        {
            var sql = CreateSql(inputObj);
            sql.Parameters.Add(new Parameter(RowCountParaName, -1, System.Data.ParameterDirection.Output));
            conn.Execute(sql, false, trans);
            int nor = (int)sql.Parameters.Single(i => i.Name == RowCountParaName).Output;

            result = default(T);
            if (nor > 0)
            {
                result = new T();
                var paras = Parameters.ToList();
                for (int i = 0; i < paras.Count; i++)
                {
                    var col = paras[i].Column;
                    col.PropertySetter(result, sql.Parameters[i].Output);
                }
            }
            return nor;
        }
    }
}
