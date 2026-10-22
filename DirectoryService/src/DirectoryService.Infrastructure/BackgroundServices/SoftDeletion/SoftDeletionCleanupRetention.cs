namespace DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;

public sealed record SoftDeletionCleanupRetention
{
    public required TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(10);
    public required TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(20);
    public required int RetentionDays { get; set; } = 60;
    public required int BatchSize { get; set; } = 500;
    public required bool Enabled { get; set; } = true;
}