using Microsoft.Extensions.DependencyInjection;

namespace MagicEastern.ADOExt
{
    public static class ServiceExt
    {
        public static DBServiceManagerBuilder AddDBServiceManger(this IServiceCollection services)
        {
            var builder = new DBServiceManagerBuilder();
            var manager = new DBServiceManager(builder);
            services.AddSingleton(manager);
            return builder;
        }
    }
}
