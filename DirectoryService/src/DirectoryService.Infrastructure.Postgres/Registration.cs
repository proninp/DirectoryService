using DirectoryService.Infrastructure.Postgres.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres;

public static class Registration
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<DbSettings>(configuration.GetSection(nameof(DbSettings)));
        return services;
    }
}