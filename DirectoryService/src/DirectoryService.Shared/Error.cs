using System.Text.Json;

namespace DirectoryService.Shared;

public record Error
{
    public IReadOnlyList<ErrorMessage> ErrorMessages { get; } = [];

    public ErrorType Type { get; }

    private Error(IEnumerable<ErrorMessage> errorMessages, ErrorType type)
    {
        ErrorMessages = errorMessages.ToArray();
        Type = type;
    }

    private Error(ErrorMessage errorMessage, ErrorType type)
        : this([errorMessage], type)
    {
    }

    public static Error None() =>
        new(new ErrorMessage(string.Empty, string.Empty), ErrorType.None);

    public static Error NotFound(string? code, string message) =>
        new(new ErrorMessage(code ?? "record.not.found", message), ErrorType.NotFound);

    public static Error Validation(string? code, string message, string? invalidField) =>
        new(new ErrorMessage(code ?? "value.is.invalid", message, invalidField), ErrorType.Validation);

    public static Error Conflict(string? code, string message) =>
        new(new ErrorMessage(code ?? "conflict", message), ErrorType.Conflict);

    public static Error Failure(string? code, string message) =>
        new(new ErrorMessage(code ?? "failure", message), ErrorType.Failure);

    public static Error NotFound(params IEnumerable<ErrorMessage> errorMessages) =>
        new(errorMessages, ErrorType.NotFound);

    public static Error Validation(params IEnumerable<ErrorMessage> errorMessages) =>
        new(errorMessages, ErrorType.Validation);

    public static Error Conflict(params IEnumerable<ErrorMessage> errorMessages) =>
        new(errorMessages, ErrorType.Conflict);

    public static Error Failure(params IEnumerable<ErrorMessage> errorMessages) =>
        new(errorMessages, ErrorType.Failure);

    public override string ToString() => JsonSerializer.Serialize(this);
}