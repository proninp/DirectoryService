using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.CreateDepartment;

public sealed class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;

    private readonly IDepartmentRepository _repository;

    private readonly ILocationRepository _locationRepository;

    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> validator,
        IDepartmentRepository repository,
        ILocationRepository locationRepository,
        ILogger<CreateDepartmentHandler> logger)
    {
        _validator = validator;
        _repository = repository;
        _locationRepository = locationRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogError(
                "Failed to create department from request {@DepartmentRequest}: {@Error}",
                command.Request, errors.ToString());
            return errors;
        }

        var allLocationsExists = await _locationRepository.AllExists(command.Request.LocationIds, cancellationToken);
        if (!allLocationsExists)
        {
            _logger.LogError(
                "Department creation error. One or several of locations not found: '{@LocationIds}'.",
                command.Request.LocationIds);
            return GeneralErrors.NotFound(
                    recordName: nameof(Location), message: "One or several locations not found.")
                .ToErrors();
        }

        Department? parent = null;

        if (command.Request.ParentId.HasValue)
        {
            var parentId = command.Request.ParentId.Value;
            parent = await _repository.GetById(parentId, cancellationToken);
            if (parent is null)
            {
                _logger.LogError("Department creation error. Parent not found: '{@ParentId}'.", parentId);
                return GeneralErrors.NotFound(parentId, nameof(Department)).ToErrors();
            }
        }

        var identifier = Identifier.Create(command.Request.Identifier);

        var departmentId = Guid.NewGuid();

        var departmentLocations = command.Request.LocationIds
            .Select(lId => new DepartmentLocation(departmentId, lId))
            .ToList();

        var department =
            parent is null
                ? Department.CreateParent(command.Request.Name, identifier.Value, departmentLocations, departmentId)
                : Department.Create(command.Request.Name, identifier.Value, parent, departmentLocations, departmentId);

        if (department.IsFailure)
        {
            _logger.LogError("Department creation error. {Error}.", department.Error.Serialize());
            return department.Error;
        }

        await _repository.Add(department.Value, cancellationToken);

        return department.Value.Id;
    }
}