using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DirectoryService.App.Extensions;

#pragma warning disable CA1812
internal sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateApiInfo(description));
        }
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