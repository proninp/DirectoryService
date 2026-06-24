using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres;

public sealed class TransactionManager(
    DirectoryServiceDbContext context,
    ILoggerFactory loggerFactory,
    ILogger<TransactionManager> logger
) : ITransactionManager
{
    public async Task<Result<ITransactionScope, Errors>> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            var transactionScopeLogger = loggerFactory.CreateLogger<TransactionScope>();
            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), transactionScopeLogger);
            return transactionScope;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while trying to start the transaction");
            return Error.Failure("begin.transaction.error", "Can not begin a new transaction")
                .ToErrors();
        }
    }

    public async Task<Result<int, Errors>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                           pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return GeneralErrors.AlreadyExists().ToErrors();
        }
        catch (Exception e)
        {
            logger.LogError(e, "n error occurred while trying to commit the transaction");
            return Error.Failure("commit.transaction.error", "Can not save changes for a new transaction")
                .ToErrors();
        }
    }
}