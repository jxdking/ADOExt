using System;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public abstract class CommandBuilderFactoryBase
    {
        public abstract ICommandBuilder<T> CreateCommandBuilder<T>(ISqlResolver sqlResolver) where T : new();
    }
}
