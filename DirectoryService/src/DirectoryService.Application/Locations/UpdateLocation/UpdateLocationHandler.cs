using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.UpdateLocation;

public sealed class UpdateLocationHandler : ICommandHandler<LocationResponse, UpdateLocationCommand>
{
    private readonly IValidator<UpdateLocationCommand> _validator;

    private readonly ILocationRepository _repository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<UpdateLocationHandler> _logger;

    public UpdateLocationHandler(
        IValidator<UpdateLocationCommand> validator,
        ILocationRepository repository,
        ITransactionManager transactionManager,
        ILogger<UpdateLocationHandler> logger)
    {
        _validator = validator;
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<LocationResponse, Errors>> Handle(
        UpdateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogWarning(
                "Failed to update location from request {@LocationRequest}: {@Error}",
                command.Request, errors.ToString());
            return errors;
        }

        var location = await _repository.GetById(command.Id, cancellationToken);
        if (location is null)
        {
            _logger.LogWarning(
                "Update location error. Location was not found by id: '{@LocationId}'.",
                command.Id);
            return GeneralErrors.NotFound(
                    command.Id, nameof(Location), $"Location {command.Id} not found")
                .ToErrors();
        }

        if (command.Request.Name is not null)
        {
            var renameResult = location.Rename(command.Request.Name);
            if (renameResult.IsFailure)
            {
                _logger.LogWarning(
                    "Location rename error. Cannot rename the location '{@LocationId}' with a new name: '{@LocationName}'.",
                    command.Id, command.Request.Name);
                return renameResult.Error;
            }
        }

        if (command.Request.AddressRequest is not null)
        {
            var addressUpdateRequest = command.Request.AddressRequest;

            var (_, isFailure, address, errors) = location.Address is null
                ? addressUpdateRequest.ToAddress()
                : addressUpdateRequest.WithAddress(location.Address);

            if (isFailure)
            {
                _logger.LogWarning(
                    "Update location error. Failed to update address for location '{@LocationId}': {@Error}",
                    command.Id, errors);
                return errors;
            }

            if (location.Address is null || !location.Address.Equals(address))
                location.UpdateAddress(address);
        }

        if (command.Request.Timezone is not null)
        {
            var newTimeZoneResult = Timezone.Create(command.Request.Timezone);
            if (newTimeZoneResult.IsFailure)
            {
                var errors = newTimeZoneResult.Error;
                _logger.LogWarning(
                    "Update location error. Failed to update timezone for location '{@LocationId}': {@Error}",
                    command.Id, errors);
                return errors;
            }

            if (!location.Timezone.Equals(newTimeZoneResult.Value))
                location.UpdateTimezone(newTimeZoneResult.Value);
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error;
        }

        return location.ToResponse();
    }
}