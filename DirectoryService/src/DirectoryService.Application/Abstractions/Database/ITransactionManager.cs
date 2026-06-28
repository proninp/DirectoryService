using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Abstractions.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Errors>> BeginTransactionAsync(CancellationToken cancellationToken);

    Task<UnitResult<Errors>> RollbackAsync(CancellationToken cancellationToken);

    Task<Result<int, Errors>> SaveChangesAsync(CancellationToken cancellationToken);
}