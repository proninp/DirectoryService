using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Cleanup;

public sealed class DeletedRecordsCleanupService(DirectoryServiceDbContext context) : IDeletedRecordsCleanupService
{
    public async Task<int> PurgeAsync<TEntity>(DateTime olderThen, int batchSize, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var totalDeletedCount = 0;
        int deletedCount;
        do
        {
            deletedCount = await context.Set<TEntity>()
                .IgnoreQueryFilters()
                .Where(e => !e.IsActive && e.DeletedAt < olderThen)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);
            totalDeletedCount += deletedCount;
        } while (!cancellationToken.IsCancellationRequested && batchSize == deletedCount);

        return totalDeletedCount;
    }
}