using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Maintenance.PurgeSoftDeletion;

public record PurgeDeletedRecordsCommand(DateTime DeleteOlderThen, int BatchSize) : IValidationCommand
{
    public object? LogContext => new { DeleteOlderThen, BatchSize };
}