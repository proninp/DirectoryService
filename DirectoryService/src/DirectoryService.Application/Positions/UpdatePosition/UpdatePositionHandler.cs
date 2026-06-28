using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Positions.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.UpdatePosition;

public sealed class UpdatePositionHandler : ICommandHandler<PositionResponse, UpdatePositionCommand>
{
    private readonly IPositionRepository _repository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<UpdatePositionHandler> _logger;

    public UpdatePositionHandler(
        IPositionRepository repository,
        ITransactionManager transactionManager,
        ILogger<UpdatePositionHandler> logger)
    {
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<PositionResponse, Errors>> Handle(
        UpdatePositionCommand command,
        CancellationToken cancellationToken)
    {
        var position = await _repository.GetById(command.Id, cancellationToken);
        if (position is null)
        {
            _logger.LogWarning(
                "Update position error. Position was not found by id: '{PositionId}'.",
                command.Id);
            return GeneralErrors.NotFound(
                    command.Id, nameof(Position), $"Position {command.Id} not found.")
                .ToErrors();
        }

        if (command.Request.Name is not null)
        {
            var existingPosition = await _repository.GetByName(command.Request.Name, cancellationToken);
            if (existingPosition is not null && existingPosition.Id != command.Id)
            {
                _logger.LogWarning(
                    "Position rename error. Position with name '{Name}' already exists.",
                    command.Request.Name);
                return GeneralErrors.AlreadyExists(
                        message: $"Position with name '{command.Request.Name}' already exists.")
                    .ToErrors();
            }

            var renameResult = position.Rename(command.Request.Name);
            if (renameResult.IsFailure)
            {
                _logger.LogWarning(
                    "Position rename error. Cannot rename position '{PositionId}' with name: '{Name}'.",
                    command.Id, command.Request.Name);
                return renameResult.Error;
            }
        }

        if (command.Request.Description is not null)
        {
            var describeResult = position.Describe(command.Request.Description);
            if (describeResult.IsFailure)
            {
                _logger.LogWarning(
                    "Position describe error. Cannot update description for position '{PositionId}'.",
                    command.Id);
                return describeResult.Error;
            }
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        return position.ToResponse();
    }
}