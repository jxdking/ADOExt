using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEastern.ADOExt.SqlServer
{
    public static class ServiceExt
    {
        public static IServiceCollection AddSqlServer(this IServiceCollection services, Func<IDbConnection> createConnection)
        {
            services.AddSingleton<IDBObjectMappingFactory, DBObjectMappingFactory>();
            services.AddTransient<IDBTableMappingFactory, DBTableMappingFactory>();
            services.AddTransient<IDBTableAdapterFactory, DBTableAdapterFactory>();

            services.AddTransient<IDBClassResolver>(_ => new DBClassResolver(createConnection));
            services.AddTransient<ISqlResolver, SqlResolver>();
            services.AddTransient<ICommandBuilderFactory, CommandBuilderFactory>();
            services.AddSingleton<IDBService, DBService>();

            return services;
        }
    }
}
