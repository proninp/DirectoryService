using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Maintenance.PurgeSoftDeletion;

public record PurgeDeletedRecordsCommand(DateTime DeleteOlderThan, int BatchSize) : IValidationCommand
{
    public object? LogContext => new { DeleteOlderThan = DeleteOlderThan, BatchSize };
}