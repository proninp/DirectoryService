namespace DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;

public sealed record SoftDeletionCleanupRetention
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(20);
    public int RetentionDays { get; init; } = 60;
    public int BatchSize { get; init; } = 500;
    public bool Enabled { get; init; } = true;
}