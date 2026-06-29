using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.DeleteLocation;

public sealed class DeleteLocationHandler : ICommandHandler<Guid, DeleteLocationCommand>
{
    private readonly ILocationRepository _repository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<DeleteLocationHandler> _logger;

    public DeleteLocationHandler(
        ILocationRepository repository,
        ITransactionManager transactionManager,
        ILogger<DeleteLocationHandler> logger)
    {
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        DeleteLocationCommand command,
        CancellationToken cancellationToken)
    {
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error;

        using var transactionScope = transactionResult.Value;

        var location = await _repository.GetByIdForUpdate(command.Id, cancellationToken);
        if (location is null)
        {
            _logger.LogError(
                "Delete location error. Location was not found by id: '{LocationId}'.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Location),
                    message: $"Location {command.Id} not found.")
                .ToErrors();
        }

        if (location.DepartmentLocations.Count > 0)
        {
            _logger.LogWarning(
                "Cannot delete location {LocationId}: it still has {Count} department link(s).",
                command.Id, location.DepartmentLocations.Count);
            return Error.Conflict(
                    "location.has.departments",
                    "Cannot delete location that is still linked to departments. Remove all department links first.")
                .ToErrors();
        }

        _repository.Delete(location);

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        if (saveChangesResult.Value == 0)
        {
            _logger.LogWarning(
                "Concurrent delete detected: Location {LocationId} was already deleted.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Location),
                    message: $"Location {command.Id} not found.")
                .ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        return location.Id;
    }
}