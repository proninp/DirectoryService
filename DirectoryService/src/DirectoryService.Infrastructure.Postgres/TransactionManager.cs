using CSharpFunctionalExtensions;
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
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(
                ex, "Concurrency conflict while saving changes for entities: {@Entities}",
                ex.Entries.Select(e => e.Entity));
            return GeneralErrors.ConcurrencyConflict().ToErrors();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            var entry = ex.Entries.Count > 0 ? ex.Entries[0] : null;
            logger.LogError(ex, "Unique violation: {ConstraintName}, Entity: {@Entity}",
                pg.ConstraintName, entry?.Entity);
            return GeneralErrors.AlreadyExists().ToErrors();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           { SqlState: PostgresErrorCodes.ForeignKeyViolation } pg)
        {
            logger.LogError(
                ex, "Foreign key constraint violated: Constraint={ConstraintName}, Table={TableName}, Detail={Detail}",
                pg.ConstraintName, pg.TableName, pg.Detail);
            return GeneralErrors.ReferenceNotFound(
                message: $"Cannot complete operation: referenced record in '{pg.TableName}' does not exist or is still referenced.",
                invalidField: pg.ConstraintName).ToErrors();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while trying to commit the transaction");
            return Error.Failure("commit.transaction.error", "Can not save changes for a new transaction")
                .ToErrors();
        }
    }
}