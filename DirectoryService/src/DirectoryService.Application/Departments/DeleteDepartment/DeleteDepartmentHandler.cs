using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public sealed class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _repository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public DeleteDepartmentHandler(
        IDepartmentRepository repository,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentHandler> logger)
    {
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        DeleteDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error;

        using var transactionScope = transactionResult.Value;

        var department = await _repository.GetByIdForUpdate(command.Id, cancellationToken);
        if (department is null)
        {
            _logger.LogError(
                "Delete department error. Department was not found by id: '{DepartmentId}'.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department),
                    message: $"Department {command.Id} not found.")
                .ToErrors();
        }

        if (department.DepartmentLocations.Count > 0)
        {
            _logger.LogWarning(
                "Cannot delete department {DepartmentId}: it still has {Count} location link(s).",
                command.Id, department.DepartmentLocations.Count);
            return Error.Conflict(
                    "department.has.locations",
                    "Cannot delete department that is still linked to locations. Remove all location links first.")
                .ToErrors();
        }

        if (department.DepartmentPositions.Count > 0)
        {
            _logger.LogWarning(
                "Cannot delete department {DepartmentId}: it still has {Count} position link(s).",
                command.Id, department.DepartmentPositions.Count);
            return Error.Conflict(
                    "department.has.positions",
                    "Cannot delete department that is still linked to positions. Remove all position links first.")
                .ToErrors();
        }

        _repository.Delete(department);

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        if (saveChangesResult.Value == 0)
        {
            _logger.LogWarning(
                "Concurrent delete detected: Department {DepartmentId} was already deleted.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department),
                    message: $"Department {command.Id} not found.")
                .ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error;

        return department.Id;
    }
}
