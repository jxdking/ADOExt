using System;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt.SqlServer
{
    public class CommandBuilderFactory : CommandBuilderFactoryBase
    {
        public override ICommandBuilder<T> CreateCommandBuilder<T>(ISqlResolver sqlResolver)
        {
            return new CommandBuilder<T>(sqlResolver);
        }
    }
}
