namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Timezone
{
    public string Value { get; init; }

    public Timezone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Timezone cannot be empty", nameof(value));

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(value);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"Invalid IANA timezone: '{value}'", nameof(value));
        }

        Value = value;
    }

    public TimeZoneInfo GetTimeZoneInfo() => TimeZoneInfo.FindSystemTimeZoneById(Value);

    public DateTime ConvertFromUtc(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime must be in UTC", nameof(utcDateTime));

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