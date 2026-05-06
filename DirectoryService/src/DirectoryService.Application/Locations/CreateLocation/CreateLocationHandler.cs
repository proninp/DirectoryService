using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Repositories;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public sealed partial class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly IValidator<CreateLocationCommand> _validator;

    private readonly ILocationRepository _locationRepository;

    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        IValidator<CreateLocationCommand> validator,
        ILocationRepository locationRepository,
        ILogger<CreateLocationHandler> logger)
    {
        _validator = validator;
        _locationRepository = locationRepository;
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Location '{@Request}' created with id '{@Id}'")]
    private static partial void LogLocationCreated(ILogger logger, CreateLocationRequest request, Guid id);

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogError(
                "Failed to create address from request {@AddressRequest}: {@Error}",
                command.Request.AddressRequest, errors.ToString());
            return errors;
        }

        var addressResult = command.Request.AddressRequest.ToAddress();

        var locationWithTheSameAddress = await _locationRepository.GetByAddress(addressResult.Value, cancellationToken);
        if (locationWithTheSameAddress is not null)
        {
            _logger.LogWarning(
                "Location with the same address already exists: {LocationAddress}", addressResult.Value);
            return Result.Failure<Guid, Errors>(GeneralErrors.AlreadyExists(
                locationWithTheSameAddress.Id,
                $"Location address already exists: '{command.Request.AddressRequest}' for " +
                $"location: '{locationWithTheSameAddress.Id}'",
                nameof(command.Request.AddressRequest)));
        }

        var locationWithTheSameName =
            await _locationRepository.GetByName(command.Request.Name, cancellationToken);

        if (locationWithTheSameName is not null)
        {
            _logger.LogWarning("Location with the same name already exists: {LocationId}", locationWithTheSameName.Id);
            return Result.Failure<Guid, Errors>(GeneralErrors.AlreadyExists(
                locationWithTheSameName.Id,
                $"Location with the name '{command.Request.Name}' already exists.",
                nameof(command.Request.Name)));
        }

        var locationResult = Location.Create(
            command.Request.Name,
            addressResult.Value,
            Timezone.Create(command.Request.Timezone).Value
        );

        if (locationResult.IsFailure)
        {
            _logger.LogError(
                "Failed to create location from request {@CreateLocationRequest}: {@Error}",
                command.Request, locationResult.Error);
            return locationResult.Error;
        }

        var id = await _locationRepository.Add(locationResult.Value, cancellationToken);
        LogLocationCreated(_logger, command.Request, id);

        return id;
    }
}