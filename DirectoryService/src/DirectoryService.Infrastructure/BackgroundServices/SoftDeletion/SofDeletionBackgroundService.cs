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

    private readonly ILogger<SoftDeletionBackgroundService> _logger;

    public SoftDeletionBackgroundService(
        IOptionsMonitor<SoftDeletionCleanupRetention> retention,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SoftDeletionBackgroundService> logger)
    {
        _retentionOption = retention;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retention = _retentionOption.CurrentValue;

        using var timer = new PeriodicTimer(retention.Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            retention = _retentionOption.CurrentValue;
            if (!retention.Enabled)
                continue;
            using var iterationCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            iterationCts.CancelAfter(retention.Timeout);

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var deletedRecordsHandler = scope.ServiceProvider
                    .GetRequiredService<ICommandHandler<PurgeDeletedRecordsResult, PurgeDeletedRecordsCommand>>();
                var command = new PurgeDeletedRecordsCommand(
                    DateTime.UtcNow - TimeSpan.FromDays(retention.RetentionDays),
                    retention.BatchSize);

                await deletedRecordsHandler.Handle(command, iterationCts.Token);
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
    }
}