using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace MagicEastern.ADOExt.Oracle
{
    public static class ServiceExt
    {
        public static IServiceCollection AddOracle(this IServiceCollection services, Func<IDbConnection> createConnection)
        {
            services.AddSingleton<IDBObjectMappingFactory, DBObjectMappingFactory>();
            services.AddTransient<IDBTableMappingFactory, DBTableMappingFactory>();
            services.AddTransient<IDBTableAdapterFactory, DBTableAdapterFactory>();

            services.AddTransient<IDBClassResolver>(_ => new DBClassResolver(createConnection));
            services.AddTransient<ISqlResolver, SqlResolver>();
            services.AddSingleton<IDBService, DBService>();

            return services;
        }

        public static DBServiceManagerBuilder AddDatabase(this DBServiceManagerBuilder builder, string name, Func<OracleConnection> createConnection)
        {
            if (name == null) { throw new ArgumentNullException("[name] is required."); }
            if (createConnection == null) { throw new ArgumentNullException("Connection constructor [createConnection] is required."); }

            var sc = new ServiceCollection();
            sc.AddOracle(createConnection);
            var sp = sc.BuildServiceProvider();
            builder.AddConfig(name, sp);
            return builder;
        }
    }
}
