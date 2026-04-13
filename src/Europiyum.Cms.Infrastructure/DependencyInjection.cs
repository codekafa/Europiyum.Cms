using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Europiyum.Cms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCmsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CmsDatabase")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'CmsDatabase' (or fallback 'DefaultConnection') is not configured.");

        services.AddDbContext<CmsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(CmsDbContext).Assembly.FullName)));

        return services;
    }
}
