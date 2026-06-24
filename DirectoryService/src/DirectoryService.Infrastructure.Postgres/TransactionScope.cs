using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres;

public sealed class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public UnitResult<Errors> Commit()
    {
        try
        {
            _transaction.Commit();
            return UnitResult.Success<Errors>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while committing the transaction");
            return UnitResult.Failure(
                Error.Failure("transaction.commit.failed", "Commit transaction error occurred")
                    .ToErrors());
        }
    }

    public UnitResult<Errors> Rollback()
    {
        try
        {
            _transaction.Rollback();
            return UnitResult.Success<Errors>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while rolling back the transaction");
            return UnitResult.Failure(
                Error.Failure("transaction.rollback.failed", "Rollback transaction error occurred")
                    .ToErrors());
        }
    }

    public void Dispose() => _transaction.Dispose();
}