using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MagicEastern.ADOExt;

namespace MagicEastern.ADOExt.Oracle
{
    public class CommandBuilderFactory : CommandBuilderFactoryBase
    {
        public override ICommandBuilder<T> CreateCommandBuilder<T>(ISqlResolver sqlResolver)
        {
            return new CommandBuilder<T>(sqlResolver);
        }
    }
}
