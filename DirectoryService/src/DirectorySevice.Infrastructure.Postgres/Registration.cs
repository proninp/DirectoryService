using DirectorySevice.Infrastructure.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DirectorySevice.Infrastructure.Postgres;

public static class Registration
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isUseSensitiveLogging = false
    )
    {
        services.Configure<DbSettings>(configuration.GetSection(nameof(DbSettings)));
        services.AddDbContext<DirectoryServiceDbContext>((provider, options) =>
            {
                var dbSettings = provider.GetRequiredService<IOptions<DbSettings>>().Value;
                options.UseNpgsql(dbSettings.ConnectionString);
                if (isUseSensitiveLogging)
                    options.EnableSensitiveDataLogging();
            }
        );
        return services;
    }
}