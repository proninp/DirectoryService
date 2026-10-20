using DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class Registration
{
    public static IServiceCollection AddSoftDeletionCleanup(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SoftDeletionCleanupRetention>(
            configuration.GetSection(nameof(SoftDeletionCleanupRetention)));
        services.AddHostedService<SoftDeletionBackgroundService>();

        return services;
    }
}