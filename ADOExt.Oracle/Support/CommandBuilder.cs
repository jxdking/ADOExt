using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEastern.ADOExt.Oracle
{
    public class CommandBuilder<T> : ICommandBuilder<T> where T : new()
    {
        private readonly ISqlResolver _SqlResolver;

        public CommandBuilder(ISqlResolver sqlResolver)
        {
            _SqlResolver = sqlResolver;
        }


        public IDBTableAdapterCommand<T> CreateDeleteCommand(DBTableAdapterContext<T> context)
        {
            if (context.PkColumns.Count() == 0)
            {
                return null;
            }
            var cmd = new NoQueryCommand<T>(
                _SqlResolver.DeleteTemplate(context),
                context.PkColumnsInfo.Select(i => new CommandParameter<T> { Column = i })
            );
            return cmd;
        }

        public IDBTableAdapterCommand<T> CreateInsertCommand(DBTableAdapterContext<T> context)
        {
            var cmd = new ReturningCommand<T>(
                _SqlResolver.InsertTemplate(context),
                context.AllColumnsInfo.Select(i => new CommandParameter<T> { Column = i, Direction = ParameterDirection.InputOutput })
            );
            return cmd;
        }

        public IDBTableAdapterCommand<T> CreateLoadCommand(DBTableAdapterContext<T> context)
        {
            if (context.PkColumns.Count() == 0)
            {
                return null;
            }
            var cmd = new QueryCommand<T>(
                _SqlResolver.LoadTemplate(context),
                context.PkColumnsInfo.Select(i => new CommandParameter<T> { Column = i })
            );
            return cmd;
        }

        public IDBTableAdapterCommand<T> CreateUpdateCommand(DBTableAdapterContext<T> context)
        {
            if (context.PkColumns.Count() == 0)
            {
                return null;
            }
            var cmd = new ReturningCommand<T>(
                _SqlResolver.UpdateTemplate(context),
                context.AllColumnsInfo.Select(i => new CommandParameter<T> { Column = i, Direction = ParameterDirection.InputOutput })
            );
            return cmd;
        }
    }
}
