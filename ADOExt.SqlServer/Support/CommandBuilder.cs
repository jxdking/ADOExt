using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class CommandBuilder<T> : ICommandBuilder<T> where T : new()
    {   
        private ISqlResolver _SqlResolver;

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
            var cmd = new NoQueryCommand<T>(
                _SqlResolver.InsertTemplate(context),
                context.InsertColumnsInfo.Select(i => new CommandParameter<T> { Column = i })
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
            var cmd = new NoQueryCommand<T>(
                _SqlResolver.UpdateTemplate(context),
                context.PkColumnsInfo.Concat(context.SetColumnsInfo).Select(i => new CommandParameter<T> { Column = i })
            );
            return cmd;
        }
    }
}
