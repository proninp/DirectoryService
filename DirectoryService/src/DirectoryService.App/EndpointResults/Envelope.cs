using System.Text.Json.Serialization;
using DirectoryService.Shared;

namespace DirectoryService.App.EndpointResults;

public record Envelope
{
    public object? Result { get; }

    public Errors? Errors { get; }

    public DateTimeOffset CreatedAt { get; }

    public bool IsSuccess => Errors is null;

    public bool IsError => !IsSuccess;

    [JsonConstructor]
    private Envelope(object? result, Errors? errors)
    {
        Result = result;
        Errors = errors;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Envelope Ok(object? result = null)
    {
        return new(result, null);
    }

    public static Envelope Error(Errors errors)
    {
        return new(null, errors);
    }
}

public record Envelope<T>
{
    public T? Result { get; }

    public Errors? Errors { get; }

    public DateTimeOffset CreatedAt { get; }

    public bool IsSuccess => Errors is null;

    public bool IsError => !IsSuccess;

    [JsonConstructor]
    private Envelope(T? result, Errors? errors)
    {
        Result = result;
        Errors = errors;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Envelope<T> Ok(T? result = default)
    {
        return new(result, null);
    }

    public static Envelope<T> Error(Errors errors)
    {
        return new(default, errors);
    }
}