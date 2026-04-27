using DirectoryService.App.Middlewares;
using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres;
using Serilog;

namespace DirectoryService.App.Extensions;

public static class ApplicationConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(configuration);

        services.AddControllers();
        services.AddOpenApi();

        services.AddGlobalExceptionHandling();

        services.AddDatabaseConfiguration(configuration);
        services.AddApplication();

        services.AddApiVersioningSupport();

        services.AddEndpointsApiExplorer();

        services.AddSwaggerConfiguration();

        return services;
    }

    public static void Configure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerConfiguration();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
    }
}