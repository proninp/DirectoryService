using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Timezone
{
    public string Value { get; } = null!;

    // EF Core Constructor
    private Timezone() { }

    [JsonConstructor]
    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone, Errors> Create(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            return Result.Failure<Timezone, Errors>(GeneralError.ValueIsRequired(nameof(Timezone)));

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return Result.Failure<Timezone, Errors>(GeneralError.ValueIsInvalid(
                nameof(Timezone), $"Invalid IANA timezone: '{timeZone}'"));
        }

        return new Timezone(timeZone);
    }

    public TimeZoneInfo GetTimeZoneInfo() => TimeZoneInfo.FindSystemTimeZoneById(Value);

    public Result<DateTime, Errors> ConvertFromUtc(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            return Result.Failure<DateTime, Errors>(
                GeneralError.ValueIsInvalid(nameof(utcDateTime), "DateTime must be in UTC"));
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, GetTimeZoneInfo());
    }

    public DateTime ConvertToUtc(DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(dateTime, GetTimeZoneInfo());
    }

    public static implicit operator string(Timezone timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        return timeZone.Value;
    }

    public override string ToString() => Value;
}