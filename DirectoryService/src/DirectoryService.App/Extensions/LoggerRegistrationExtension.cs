using Serilog;

namespace DirectoryService.App.Extensions;

internal static class LoggerRegistrationExtension
{
    public static IHostBuilder AddLogging(this IHostBuilder builder, IConfiguration configuration)
    {
        return builder
            .UseSerilog((_, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom
                        .Configuration(configuration)
                        .Enrich.FromLogContext();
                }
            );
    }
}