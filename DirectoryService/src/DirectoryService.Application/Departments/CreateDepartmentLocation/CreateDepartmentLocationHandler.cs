using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.CreateDepartmentLocation;

public sealed class
    CreateDepartmentLocationHandler : ICommandHandler<DepartmentLocationResponse, CreateDepartmentLocationCommand>
{
    private readonly IDepartmentRepository _departmentRepository;

    private readonly ILocationRepository _locationRepository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<CreateDepartmentLocationHandler> _logger;

    public CreateDepartmentLocationHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<CreateDepartmentLocationHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<DepartmentLocationResponse, Errors>> Handle(
        CreateDepartmentLocationCommand command,
        CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetById(command.DepartmentId, cancellationToken);

        if (department is null)
        {
            _logger.LogWarning(
                "Create department-location error. Department was not found by id: '{@DepartmentId}'.",
                command.DepartmentId);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department), message: $"Department {command.DepartmentId} not found.")
                .ToErrors();
        }

        var departmentLocation = department.DepartmentLocations
            .FirstOrDefault(dl => dl.LocationId == command.LocationId);

        if (departmentLocation is not null)
        {
            _logger.LogWarning(
                "Create department-location error. Relation already exists " +
                $"for DepartmentId {command.DepartmentId} and LocationId {command.LocationId}.");
            return GeneralErrors.AlreadyExists(
                    departmentLocation.Id,
                    $"Department {command.DepartmentId} and Location {command.LocationId} relation already exists")
                .ToErrors();
        }

        var location = await _locationRepository.GetById(command.LocationId, cancellationToken);
        if (location is null)
        {
            _logger.LogError(
                $"Create department-location error. Location was not found by id: '{command.LocationId}'.");
            return GeneralErrors.NotFound(
                    recordName: nameof(Location), message: $"Location {command.LocationId} not found.")
                .ToErrors();
        }

        department.AddLocation(command.LocationId);

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            if (saveChangesResult.Error.IsUniqueViolation())
            {
                return GeneralErrors.AlreadyExists(
                        message:
                        $"Department {command.DepartmentId} and Location {command.LocationId} relation already exists.")
                    .ToErrors();
            }

            return saveChangesResult.Error;
        }

        return department.ToDepartmentLocationResponse();
    }
}