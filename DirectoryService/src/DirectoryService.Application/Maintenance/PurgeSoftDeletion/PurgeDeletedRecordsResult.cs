using DirectoryService.Shared;

namespace DirectoryService.Application.Maintenance.PurgeSoftDeletion;

public sealed record PurgeDeletedRecordsResult(
    int DepartmentsPurged,
    int LocationsPurged,
    int PositionsPurged,
    PurgeOutcome Outcome,
    Error? Error = null);