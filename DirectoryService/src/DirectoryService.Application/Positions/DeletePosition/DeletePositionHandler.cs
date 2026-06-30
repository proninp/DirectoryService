using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.DeletePosition;

public sealed class DeletePositionHandler : ICommandHandler<Guid, DeletePositionCommand>
{
    private readonly IPositionRepository _repository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<DeletePositionHandler> _logger;

    public DeletePositionHandler(
        IPositionRepository repository,
        ITransactionManager transactionManager,
        ILogger<DeletePositionHandler> logger)
    {
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        DeletePositionCommand command,
        CancellationToken cancellationToken)
    {
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error;

        using var transactionScope = transactionResult.Value;

        var position = await _repository.GetByIdForUpdate(command.Id, cancellationToken);
        if (position is null)
        {
            _logger.LogError(
                "Delete position error. Position was not found by id: '{PositionId}'.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Position),
                    message: $"Position {command.Id} not found.")
                .ToErrors();
        }

        if (position.DepartmentPositions.Count > 0)
        {
            _logger.LogWarning(
                "Cannot delete position {PositionId}: it still has {Count} department link(s).",
                command.Id, position.DepartmentPositions.Count);
            return Error.Conflict(
                    "position.has.departments",
                    "Cannot delete position that is still linked to departments. Remove all department links first.")
                .ToErrors();
        }

        _repository.Delete(position);

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        if (saveChangesResult.Value == 0)
        {
            _logger.LogWarning(
                "Concurrent delete detected: Position {PositionId} was already deleted.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Position),
                    message: $"Position {command.Id} not found.")
                .ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        return position.Id;
    }
}