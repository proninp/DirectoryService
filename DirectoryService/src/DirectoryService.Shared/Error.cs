using System.Text.Json;
using CSharpFunctionalExtensions;

namespace DirectoryService.Shared;

public record Error
{
    public ErrorMessage ErrorMessage { get; }

    public ErrorType ErrorType { get; }

    private Error(string code, string message, string? invalidField = null, ErrorType errorType = ErrorType.None)
        : this(new ErrorMessage(code, message, invalidField), errorType)
    {
    }

    private Error(ErrorMessage errorMessage, ErrorType errorType = ErrorType.None)
    {
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static Error None() =>
        new(new ErrorMessage(string.Empty, string.Empty));

    public static Error NotFound(string? code, string message) =>
        new(new ErrorMessage(code ?? "record.not.found", message), ErrorType.NotFound);

    public static Error Validation(string? code, string message, string? invalidField) =>
        new(new ErrorMessage(code ?? "value.is.invalid", message, invalidField), ErrorType.Validation);

    public static Error Conflict(string? code, string message, string? invalidField = null) =>
        new(new ErrorMessage(code ?? "conflict", message, invalidField), ErrorType.Conflict);

    public static Error Failure(string? code, string message) =>
        new(new ErrorMessage(code ?? "failure", message), ErrorType.Failure);

    public override string ToString() => JsonSerializer.Serialize(this);
}