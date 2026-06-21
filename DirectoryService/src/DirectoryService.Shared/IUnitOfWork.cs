using CSharpFunctionalExtensions;

namespace DirectoryService.Shared;

public interface IUnitOfWork
{
    Task<Result<int, Errors>> TryCommitAsync(CancellationToken cancellationToken = default);

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}