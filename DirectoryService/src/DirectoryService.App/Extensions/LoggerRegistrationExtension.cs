using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.App.Extensions;

internal static class LoggerRegistrationExtension
{
    public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddSerilog((serviceProvider, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(configuration)
                        .ReadFrom.Services(serviceProvider)
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails();
                }
            );
    }
}