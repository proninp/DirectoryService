using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Maintenance.PurgeSoftDeletion;

#pragma warning disable CA1873
public sealed class PurgeDeletedRecordsHandler : ICommandHandler<PurgeDeletedRecordsResult, PurgeDeletedRecordsCommand>
{
    public PurgeDeletedRecordsHandler(
        IDeletedRecordsCleanupService cleanupService, ILogger<PurgeDeletedRecordsHandler> logger)
    {
        _cleanupService = cleanupService;
        _logger = logger;
    }

    private readonly IDeletedRecordsCleanupService _cleanupService;
    private readonly ILogger<PurgeDeletedRecordsHandler> _logger;

    public async Task<Result<PurgeDeletedRecordsResult, Errors>> Handle(
        PurgeDeletedRecordsCommand command, CancellationToken cancellationToken)
    {
        int departmentsPurged = 0, locationsPurged = 0, positionsPurged = 0;
        try
        {
            departmentsPurged =
                await _cleanupService.PurgeAsync<Department>(command.DeleteOlderThen, command.BatchSize,
                    cancellationToken);
            locationsPurged =
                await _cleanupService.PurgeAsync<Location>(command.DeleteOlderThen, command.BatchSize,
                    cancellationToken);
            positionsPurged =
                await _cleanupService.PurgeAsync<Position>(command.DeleteOlderThen, command.BatchSize,
                    cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Purge run stopped by time budget, partial progress kept.");
            var purgeDeletedRecordsCancelResult =
                new PurgeDeletedRecordsResult(departmentsPurged, locationsPurged, positionsPurged,
                    PurgeOutcome.Cancelled);
            return Result.Success<PurgeDeletedRecordsResult, Errors>(purgeDeletedRecordsCancelResult);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Cleanup service error.");
            var error = GeneralErrors.Failure($"Cleanup service error occurs: {e.Message}");
            var failedResult =
                new PurgeDeletedRecordsResult(
                    departmentsPurged, locationsPurged, positionsPurged, PurgeOutcome.Failed, error);
            return Result.Success<PurgeDeletedRecordsResult, Errors>(failedResult);
        }

        var purgeDeletedRecordsResult =
            new PurgeDeletedRecordsResult(departmentsPurged, locationsPurged, positionsPurged, PurgeOutcome.Completed);

        _logger.LogInformation(
            "Purge run completed: {DepartmentsPurged} departments, {LocationsPurged} locations, {PositionsPurged} positions.",
            purgeDeletedRecordsResult.DepartmentsPurged, purgeDeletedRecordsResult.LocationsPurged,
            purgeDeletedRecordsResult.PositionsPurged);

        return purgeDeletedRecordsResult;
    }
}