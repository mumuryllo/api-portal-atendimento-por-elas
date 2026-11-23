using Microsoft.EntityFrameworkCore;
using portal_por_elas.Data.Context;

namespace portal_por_elas.Extensions
{
    public static class DatabaseServiceExtension
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PortalPorElasDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}
