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
    }
}
