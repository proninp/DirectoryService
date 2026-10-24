using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Maintenance.PurgeSoftDeletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;

public sealed class SoftDeletionBackgroundService : BackgroundService
{
    private readonly IOptionsMonitor<SoftDeletionCleanupRetention> _retentionOption;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    private readonly TimeProvider _timeProvider;

    private readonly ILogger<SoftDeletionBackgroundService> _logger;

    public SoftDeletionBackgroundService(
        IOptionsMonitor<SoftDeletionCleanupRetention> retention,
        IServiceScopeFactory serviceScopeFactory,
        TimeProvider timeProvider,
        ILogger<SoftDeletionBackgroundService> logger)
    {
        _retentionOption = retention;
        _serviceScopeFactory = serviceScopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retention = _retentionOption.CurrentValue;

        using var timer = new PeriodicTimer(retention.Interval);

        do
        {
            try
            {
                retention = _retentionOption.CurrentValue;

                if (timer.Period != retention.Interval)
                    timer.Period = retention.Interval;

                if (!retention.Enabled)
                    continue;

                using var iterationCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                iterationCts.CancelAfter(retention.Timeout);

                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var deletedRecordsHandler = scope.ServiceProvider
                    .GetRequiredService<ICommandHandler<PurgeDeletedRecordsResult, PurgeDeletedRecordsCommand>>();
                var command = new PurgeDeletedRecordsCommand(
                    _timeProvider.GetUtcNow().UtcDateTime - TimeSpan.FromDays(retention.RetentionDays),
                    retention.BatchSize);

                var deletionResult = await deletedRecordsHandler.Handle(command, iterationCts.Token);
                if (deletionResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Soft deletion purge command was rejected, iteration skipped. Errors: {@Errors}",
                        deletionResult.Error);
                }
                else
                {
                    LogPurgeSummary(deletionResult.Value);
                }
            }
            catch (OptionsValidationException e) when (e.OptionsType == typeof(SoftDeletionCleanupRetention))
            {
                _logger.LogError(
                    "{OptionsName} options are invalid, iteration skipped. Failures: {@Failures}",
                    nameof(SoftDeletionCleanupRetention),
                    e.Failures);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred during SoftDeletionBackgroundService");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private void LogPurgeSummary(PurgeDeletedRecordsResult result)
    {
        var level = result.Outcome == PurgeOutcome.Completed ? LogLevel.Information : LogLevel.Warning;

        if (!_logger.IsEnabled(level))                                                                                                                                                                                           
            return;     
        
        _logger.Log(
            level,
            "Soft deletion purge {Outcome}: {TotalPurged} records purged " +
            "(departments: {DepartmentsPurged}, locations: {LocationsPurged}, positions: {PositionsPurged}). " +
            "Entity sets processed: {EntitySetsProcessed}, skipped: {EntitySetsSkipped}, failed: {EntitySetsFailed}. " +
            "Error: {@Error}",
            result.Outcome,
            result.TotalPurged,
            result.DepartmentsPurged,
            result.LocationsPurged,
            result.PositionsPurged,
            result.EntitySetsProcessed,
            result.EntitySetsSkipped,
            result.EntitySetsFailed,
            result.Error);
    }
}