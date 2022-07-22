using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt
{
    public static class ServiceExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configs">List of Database configuration. The first configuration will be the default one.</param>
        /// <returns></returns>
        public static IServiceCollection AddDBServiceManger(this IServiceCollection services, IEnumerable<DBServiceConfig> configs)
        {
            services.AddSingleton(_=>new DBServiceManager(configs));
            return services;
        }
    }
}
