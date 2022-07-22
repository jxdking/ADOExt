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
        public IList<CommandParameter<T>> Parameters;

        public CommandBase(string commandText, IEnumerable<CommandParameter<T>> parameters)
        {
            CommandText = commandText;
            Parameters = parameters.ToList();
        }

        public abstract int Execute(T inputObj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null);

        protected virtual Sql CreateSql(T obj)
        {
            var ps = Parameters.Select(i => CreateInputParameter(i.Column, obj, i.Direction));
            return new Sql(CommandText, ps);
        }

        private Parameter CreateInputParameter(IDBColumnMapping<T> colInfo, T obj, ParameterDirection direction)
        {
            return new Parameter(colInfo.ColumnName, colInfo.PropertyGetter(obj), direction);
        }
    }
}
