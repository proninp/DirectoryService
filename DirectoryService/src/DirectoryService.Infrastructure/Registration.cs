using DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class Registration
{
    public static IServiceCollection AddSoftDeletionCleanup(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SoftDeletionCleanupRetention>()
            .Bind(configuration.GetSection(nameof(SoftDeletionCleanupRetention)))
            .ValidateOnStart();

        services.AddHostedService<SoftDeletionBackgroundService>();

        return services;
    }
}