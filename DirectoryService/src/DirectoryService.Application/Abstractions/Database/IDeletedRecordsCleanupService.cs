using DirectoryService.Domain.Entities.Abstractions;

namespace DirectoryService.Application.Abstractions.Database;

public interface IDeletedRecordsCleanupService
{
    Task<int> PurgeAsync<TEntity>(DateTime olderThen, int batchSize, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;
}