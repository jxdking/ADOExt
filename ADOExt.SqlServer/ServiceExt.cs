using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;

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

            services.AddSingleton(typeof(ISqlDeleteTemplate<>), typeof(SqlDeleteTemplate<>));
            services.AddSingleton(typeof(ISqlLoadTemplate<>), typeof(SqlLoadTemplate<>));
            services.AddSingleton(typeof(ISqlInsertTemplate<>), typeof(SqlInsertTemplate<>));
            services.AddSingleton(typeof(ISqlUpdateTemplate<>), typeof(SqlUpdateTemplate<>));

            services.AddSingleton<ISqlResolver, SqlResolver>();

            services.AddSingleton<ConnectionFactory>((sp) =>
            {
                return () =>
                {
                    var conn = createConnection();
                    conn.Open();
                    return new DBConnectionWrapper(conn, sp.GetService<IDBService>(), () => new SqlCommand());
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
