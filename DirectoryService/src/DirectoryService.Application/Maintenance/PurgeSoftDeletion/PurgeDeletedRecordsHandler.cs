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
    private const int PurgeEntitySetsCount = 3;

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
        var processedSets = 0;
        try
        {
            departmentsPurged =
                await _cleanupService.PurgeAsync<Department>(command.DeleteOlderThan, command.BatchSize,
                    cancellationToken);
            processedSets++;
            locationsPurged =
                await _cleanupService.PurgeAsync<Location>(command.DeleteOlderThan, command.BatchSize,
                    cancellationToken);
            processedSets++;
            positionsPurged =
                await _cleanupService.PurgeAsync<Position>(command.DeleteOlderThan, command.BatchSize,
                    cancellationToken);
            processedSets++;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Purge run stopped by time budget, partial progress kept.");

            // Прерванный набор считается пропущенным: удаление в нём могло пройти лишь частично
            var purgeDeletedRecordsCancelResult =
                new PurgeDeletedRecordsResult(departmentsPurged, locationsPurged, positionsPurged,
                    PurgeOutcome.Cancelled, processedSets, PurgeEntitySetsCount - processedSets, 0);
            return Result.Success<PurgeDeletedRecordsResult, Errors>(purgeDeletedRecordsCancelResult);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Cleanup service error.");
            var error = GeneralErrors.Failure($"Cleanup service error occurs: {e.Message}");

            // Упавший набор считается ошибкой, все наборы после него — пропущенными
            var failedResult =
                new PurgeDeletedRecordsResult(
                    departmentsPurged, locationsPurged, positionsPurged, PurgeOutcome.Failed,
                    processedSets, PurgeEntitySetsCount - processedSets - 1, 1, error);
            return Result.Success<PurgeDeletedRecordsResult, Errors>(failedResult);
        }

        var purgeDeletedRecordsResult =
            new PurgeDeletedRecordsResult(departmentsPurged, locationsPurged, positionsPurged, PurgeOutcome.Completed,
                processedSets, 0, 0);

        _logger.LogInformation(
            "Purge run completed: {DepartmentsPurged} departments, {LocationsPurged} locations, {PositionsPurged} positions.",
            purgeDeletedRecordsResult.DepartmentsPurged, purgeDeletedRecordsResult.LocationsPurged,
            purgeDeletedRecordsResult.PositionsPurged);

        return purgeDeletedRecordsResult;
    }
}