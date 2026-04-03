using DirectoryService.Infrastructure.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres;

public static class Registration
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isUseSensitiveLogging = false
    )
    {
        services.Configure<DbSettings>(configuration.GetSection(nameof(DbSettings)));
        services.AddDbContextPool<DirectoryServiceDbContext>((provider, options) =>
            {
                var dbSettings = provider.GetRequiredService<IOptions<DbSettings>>().Value;
                options.UseNpgsql(dbSettings.ConnectionString);
                if (isUseSensitiveLogging)
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                    options.UseLoggerFactory(CreateLoggerFactory());
                }
            }
        );
        return services;
    }

    private static ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => builder.AddConsole());
}