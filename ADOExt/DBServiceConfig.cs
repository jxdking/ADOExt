using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class DBServiceConfig
    {
        public string Name { get; set; }
        public Action<IServiceCollection> ConfigServices { get; set; }
    }
}
