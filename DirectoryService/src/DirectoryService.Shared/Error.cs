using System.Text.Json;
using CSharpFunctionalExtensions;

namespace DirectoryService.Shared;

public record Error : ICombine
{
    public IReadOnlyList<ErrorMessage> ErrorMessages { get; } = [];

    public ErrorType Type { get; }

    private Error(IEnumerable<ErrorMessage> errorMessages, ErrorType type)
    {
        ErrorMessages = [.. errorMessages];
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

    public ICombine Combine(ICombine value)
    {
        var other = (Error)value;
        return new Error(ErrorMessages.Union(other.ErrorMessages), Type);
    }
}