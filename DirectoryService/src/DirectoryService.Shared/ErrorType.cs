using System.Text.Json.Serialization;

namespace DirectoryService.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Failure,
    Conflict
}