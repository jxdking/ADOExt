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
        public static IServiceCollection AddADOExtSqlServer(this IServiceCollection services, Func<IDbConnection> createConnection)
        {
            return services.AddSingleton(
                new ResolverProvider(
                    new DBClassResolver(createConnection), new SqlResolver(), new CommandBuilderFactory()
                )
            );
        }
    }
}
