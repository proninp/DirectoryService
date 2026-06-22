using DirectoryService.Application.Abstractions;
using DirectoryService.Infrastructure.Postgres.Options;
using DirectoryService.Shared;
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

        services.AddDbContext<IUnitOfWork, DirectoryServiceDbContext>();

        services.Scan(scan => scan
            .FromAssemblyOf<DirectoryServiceDbContext>()
            .AddClasses(classes => classes.AssignableTo<IRepository>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}