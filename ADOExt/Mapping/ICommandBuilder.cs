using System;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public interface ICommandBuilder<T> where T: new ()
    {
        IDBTableAdapterCommand<T> CreateInsertCommand(DBTableAdapterContext<T> context);
        IDBTableAdapterCommand<T> CreateUpdateCommand(DBTableAdapterContext<T> context);
        IDBTableAdapterCommand<T> CreateDeleteCommand(DBTableAdapterContext<T> context);
        IDBTableAdapterCommand<T> CreateLoadCommand(DBTableAdapterContext<T> context);
    }
}
