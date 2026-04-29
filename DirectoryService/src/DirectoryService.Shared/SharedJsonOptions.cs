using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirectoryService.Shared;

internal static class SharedJsonOptions
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
}