using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi;

namespace DirectoryService.App.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
            {
                var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateApiInfo(description));
                }
            }
        );

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant()
                    );
                }
            }
        );

        return app;
    }

    private static OpenApiInfo CreateApiInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "Directory Service API",
            Version = description.ApiVersion.ToString(),
            Contact = new OpenApiContact
            {
                Name = "Velni",
                Email = "stackcrawler@gmail.com"
            }
        };

        if (description.IsDeprecated)
        {
            info.Description += " (Deprecated)";
        }

        return info;
    }
}