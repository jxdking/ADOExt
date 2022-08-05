using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;

namespace MagicEastern.ADOExt.SqlServer
{
    public static class ServiceExt
    {
        public static IServiceCollection AddSqlServer(this IServiceCollection services, Func<IDbConnection> createConnection)
        {
            services.AddSingleton(typeof(IDBObjectMapping<>), typeof(DBObjectMapping<>));
            services.AddSingleton(typeof(IDBTableMapping<>), typeof(DBTableMapping<>));
            services.AddSingleton(typeof(DBTableAdapterContext<>));
            services.AddSingleton(typeof(IDBTableAdapter<>), typeof(DBTableAdapter<>));

            services.AddSingleton(typeof(SqlDeleteTemplateBase<>), typeof(SqlDeleteTemplate<>));
            services.AddSingleton(typeof(SqlLoadTemplateBase<>), typeof(SqlLoadTemplate<>));
            services.AddSingleton(typeof(SqlInsertTemplateBase<>), typeof(SqlInsertTemplate<>));
            services.AddSingleton(typeof(SqlUpdateTemplateBase<>), typeof(SqlUpdateTemplate<>));

            services.AddSingleton<IDBCommandBuilder, DBCommandBuilder>();
            services.AddSingleton<ISqlResolver, SqlResolver>();

            services.AddSingleton<ConnectionFactory>((sp) =>
            {
                return () =>
                {
                    var conn = createConnection();
                    conn.Open();
                    return new DBConnectionWrapper(conn, sp.GetService<IDBService>());
                };
            });
            services.AddSingleton<IDBService, DBService>();

            return services;
        }

        public static DBServiceManagerBuilder AddDatabase(this DBServiceManagerBuilder builder, string name, Func<IDbConnection> createConnection)
        {
            if (name == null) { throw new ArgumentNullException("[name] is required."); }
            if (createConnection == null) { throw new ArgumentNullException("Connection constructor [createConnection] is required."); }

            var sc = new ServiceCollection();
            sc.AddSqlServer(createConnection);
            var sp = sc.BuildServiceProvider();
            builder.AddConfig(name, sp);
            return builder;
        }
    }
}
