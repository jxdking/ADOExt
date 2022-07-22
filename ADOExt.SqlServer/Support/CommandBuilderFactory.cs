using System;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class CommandBuilderFactory : ICommandBuilderFactory
    {
        public ICommandBuilder<T> CreateCommandBuilder<T>(ISqlResolver sqlResolver) where T:new()
        {
            return new CommandBuilder<T>(sqlResolver);
        }
    }
}
