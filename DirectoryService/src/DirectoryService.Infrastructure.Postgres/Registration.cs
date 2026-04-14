using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure.Postgres.Options;
using DirectoryService.Infrastructure.Postgres.Repositories;
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

        services.AddDbContext<DirectoryServiceDbContext>();

        services.AddScoped<ILocationRepository, LocationRepository>();

        return services;
    }
}