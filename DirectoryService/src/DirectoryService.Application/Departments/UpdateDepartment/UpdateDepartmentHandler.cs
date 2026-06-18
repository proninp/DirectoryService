using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public sealed class UpdateDepartmentHandler : ICommandHandler<DepartmentResponse, UpdateDepartmentCommand>
{
    private readonly IValidator<UpdateDepartmentCommand> _validator;

    private readonly IDepartmentRepository _repository;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ILocationRepository _locationRepository;

    private readonly ILogger<UpdateDepartmentHandler> _logger;

    public UpdateDepartmentHandler(
        IValidator<UpdateDepartmentCommand> validator,
        IUnitOfWork unitOfWork,
        IDepartmentRepository repository,
        ILocationRepository locationRepository,
        ILogger<UpdateDepartmentHandler> logger)
    {
        _validator = validator;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _locationRepository = locationRepository;
        _logger = logger;
    }

    public async Task<Result<DepartmentResponse, Errors>> Handle(
        UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogError(
                "Failed to update department from request {@DepartmentRequest}: {@Error}",
                request, errors.ToString());
            return errors;
        }

        var department = await _repository.GetById(command.Id, cancellationToken);
        if (department is null)
        {
            _logger.LogError(
                "Update department error. Department was not found by id: '{@DepartmentId}'.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department), message: $"Department {command.Id} not found.")
                .ToErrors();
        }

        if (request.Name is not null)
        {
            var renameResult = department.Rename(request.Name);
            if (renameResult.IsFailure)
            {
                _logger.LogError(
                    "Department rename error. Cannot rename the department {@DepartmentId} with a new name: '{@DepartmentName}'.",
                    command.Id, request.Name);
                return renameResult.Error;
            }
        }

        if (request.LocationIds is not null)
        {
            var allLocationsExist = await _locationRepository
                .AllExists(request.LocationIds, cancellationToken);
            if (!allLocationsExist)
            {
                _logger.LogError(
                    "Update department error. One or several of locations not found: '{@LocationIds}'.",
                    request.LocationIds);
                return GeneralErrors.NotFound(
                        recordName: nameof(Location), message: "One or several locations not found.")
                    .ToErrors();
            }

            var requestIds = request.LocationIds.ToHashSet();
            var currentIds = department.DepartmentLocations
                .Select(dl => dl.LocationId)
                .ToHashSet();

            foreach (var locationId in currentIds.Except(requestIds))
            {
                var removeLocationResult = department.RemoveLocation(locationId);
                if (removeLocationResult.IsFailure)
                {
                    _logger.LogError(
                        "Update department error. Failed to remove location '{@LocationId}': {@Error}",
                        locationId, removeLocationResult.Error);
                    return removeLocationResult.Error;
                }
            }

            foreach (var locationId in requestIds.Except(currentIds))
            {
                var addLocationResult = department.AddLocation(locationId);
                if (addLocationResult.IsFailure)
                {
                    _logger.LogError(
                        "Update department error. Failed to add location '{@LocationId}': {@Error}",
                        locationId, addLocationResult.Error);
                    _logger.LogError(addLocationResult.Error.Serialize());
                    return addLocationResult.Error;
                }
            }
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return department.ToResponse();
    }
}