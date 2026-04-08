using System.Text.Json;

namespace DirectoryService.Shared;

public record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public string? InvalidField { get; }

    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public static Error NotFound(string? code, string message) =>
        new(code ?? "record.not.found", message, ErrorType.NotFound);

    public static Error Validation(string? code, string message, string? invalidField) =>
        new(code ?? "value.is.invalid", message, ErrorType.Validation, invalidField);

    public static Error Conflict(string? code, string message) =>
        new(code ?? "conflict", message, ErrorType.Conflict);

    public static Error Failure(string? code, string message) =>
        new(code ?? "failure", message, ErrorType.Failure);

    public override string ToString() => JsonSerializer.Serialize(this);
}