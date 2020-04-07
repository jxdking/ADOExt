using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    /// <summary>
    /// The Command object with default behavior. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CommandBase<T> : IDBTableAdapterCommand<T> where T : new()
    {
        public string CommandText;
        public IEnumerable<CommandParameter<T>> Parameters;

        public CommandBase(string commandText, IEnumerable<CommandParameter<T>> parameters)
        {
            CommandText = commandText;
            Parameters = parameters;
        }

        /// <summary>
        /// Underline it calls Command.Execute and return the number of row effected. No "returning"(in Oracle) or "output"(in SqlServer) parameter involves.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public virtual int Execute(ref T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            var sql = CreateSql(obj);
            int ret = conn.Execute(sql, false, trans);
            return ret;
        }

        public virtual Sql CreateSql(T obj)
        {
            var ps = Parameters.Select(i => CreateInputParameter(i.Column, obj, i.Direction)).ToList();
            return new Sql(CommandText, ps);
        }

        private Parameter CreateInputParameter(IDBColumnMapping<T> colInfo, T obj, ParameterDirection direction)
        {
            return new Parameter(colInfo.ColumnName, colInfo.PropertyGetter(obj), direction);
        }
    }
}