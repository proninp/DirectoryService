using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.DeleteDepartmentLocation;

public sealed class DeleteDepartmentLocationHandler
    : ICommandHandler<DepartmentLocationResponse, DeleteDepartmentLocationCommand>
{
    private readonly IDepartmentRepository _departmentRepository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<DeleteDepartmentLocationHandler> _logger;

    public DeleteDepartmentLocationHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentLocationHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<DepartmentLocationResponse, Errors>> Handle(
        DeleteDepartmentLocationCommand command,
        CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetById(command.DepartmentId, cancellationToken);

        if (department is null)
        {
            _logger.LogWarning(
                "Delete department-location error. Department was not found by id: '{DepartmentId}'.",
                command.DepartmentId);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department), message: $"Department {command.DepartmentId} not found.")
                .ToErrors();
        }

        var departmentLocation = department.DepartmentLocations
            .FirstOrDefault(dl => dl.LocationId == command.LocationId);

        if (departmentLocation is null)
        {
            _logger.LogWarning(
                "Delete department-location error. Relation does not exist " +
                "for DepartmentId {@DepartmentId} and LocationId {@LocationId}.",
                command.DepartmentId, command.LocationId);
            return GeneralErrors.NotFound(
                    recordName: nameof(DepartmentLocation),
                    message:
                    $"Department {command.DepartmentId} and Location {command.LocationId} relation does not exist")
                .ToErrors();
        }

        var removeResult = department.RemoveLocation(command.LocationId);
        if (removeResult.IsFailure)
        {
            _logger.LogWarning(
                "Remove department-location failed. Cannot remove the last location {LocationId} " +
                "from department {DepartmentId}. Department must have at least one location.",
                command.LocationId, command.DepartmentId);
            return removeResult.Error;
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error;
        }

        var commitResult = saveChangesResult.Value;
        if (commitResult == 0)
        {
            _logger.LogWarning(
                "Concurrent delete detected: DepartmentLocation for " +
                "DepartmentId {DepartmentId} and LocationId {LocationId} relation does not exist.",
                command.DepartmentId, command.LocationId);
            return GeneralErrors.NotFound(
                    message:
                    $"Department {command.DepartmentId} and Location {command.LocationId} relation does not exist.")
                .ToErrors();
        }

        return department.ToDepartmentLocationResponse();
    }
}