using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEastern.ADOExt.Oracle
{
    public static class ServiceExt
    {
        public static IServiceCollection AddADOExtOracle(this IServiceCollection services, Func<IDbConnection> createConnection)
        {
            return services.AddSingleton(
                new ResolverProvider(
                    new DBClassResolver(createConnection), new SqlResolver(), new CommandBuilderFactory()
                )
            );
        }
    }
}
