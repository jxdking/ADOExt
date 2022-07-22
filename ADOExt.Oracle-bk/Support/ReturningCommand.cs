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

        private string RowCountParaName = "sql_nor";

        public override int Execute(ref T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            var sql = CreateSql(obj);
            sql.Parameters.Add(new Parameter(RowCountParaName, -1, System.Data.ParameterDirection.Output));
            conn.Execute(sql, false, trans);
            int nor = (int)sql.Parameters.Single(i => i.Name == RowCountParaName).Output;

            if (nor > 0)
            {
                var paras = Parameters.ToList();
                for (int i = 0; i < paras.Count; i++)
                {
                    var col = paras[i].Column;
                    col.PropertySetter(obj, sql.Parameters[i].Output);
                }
            }
            return nor;
        }
    }
}
