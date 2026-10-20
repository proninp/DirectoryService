namespace DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;

public sealed record SoftDeletionCleanupRetention
{
    public required TimeSpan Interval { get; set; }
    public required TimeSpan Timeout { get; set; }
    public required int RetentionDays { get; set; }
    public required int BatchSize { get; set; } = 500;
    public required bool Enabled { get; set; } = true;
}