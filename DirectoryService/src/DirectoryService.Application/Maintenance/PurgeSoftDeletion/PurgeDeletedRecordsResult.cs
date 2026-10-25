using DirectoryService.Shared;

namespace DirectoryService.Application.Maintenance.PurgeSoftDeletion;

public sealed record PurgeDeletedRecordsResult(
    int DepartmentsPurged,
    int LocationsPurged,
    int PositionsPurged,
    PurgeOutcome Outcome,
    int EntitySetsProcessed,
    int EntitySetsSkipped,
    int EntitySetsFailed,
    Error? Error = null)
{
    public int TotalPurged => DepartmentsPurged + LocationsPurged + PositionsPurged;
}