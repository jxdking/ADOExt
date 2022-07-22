using System;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public interface ICommandBuilderFactory
    {
        ICommandBuilder<T> CreateCommandBuilder<T>(ISqlResolver sqlResolver) where T : new();
    }
}
