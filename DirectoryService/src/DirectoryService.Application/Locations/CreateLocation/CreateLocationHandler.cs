using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public sealed class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;

    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationRepository locationRepository, ILogger<CreateLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var addressResult = command.Request.AddressRequest.ToAddress();
        if (addressResult.IsFailure)
        {
            _logger.LogError(
                "Failed to create address from request {@AddressRequest}: {@Error}",
                command.Request.AddressRequest, addressResult.Error);
            return addressResult.Error;
        }

        var locationWithTheSameAddress = await _locationRepository.GetByAddress(addressResult.Value, cancellationToken);
        if (locationWithTheSameAddress is not null)
        {
            _logger.LogWarning(
                "Location with the same address already exists: {LocationAddress}", addressResult.Value);
            return Result.Failure<Guid, Errors>(GeneralError.AlreadyExists(
                locationWithTheSameAddress.Id,
                $"Location address already exists: '{command.Request.AddressRequest}'." +
                $"Location: {locationWithTheSameAddress.Id}",
                nameof(command.Request.AddressRequest)));
        }

        var locationWithTheSameName =
            await _locationRepository.GetByName(command.Request.Name, cancellationToken);

        if (locationWithTheSameName is not null)
        {
            _logger.LogWarning("Location with the same name already exists: {LocationId}", locationWithTheSameName.Id);
            return Result.Failure<Guid, Errors>(GeneralError.AlreadyExists(
                locationWithTheSameName.Id,
                $"Location with the name '{command.Request.Name}' already exists",
                nameof(command.Request.Name)));
        }

        var locationResult = Location.Create(
            command.Request.Name,
            addressResult.Value,
            new Timezone(command.Request.Timezone)
        );

        if (locationResult.IsFailure)
        {
            _logger.LogError(
                "Failed to create location from request {@CreateLocationRequest}: {@Error}",
                command.Request, locationResult.Error);
            return locationResult.Error;
        }

        var id = await _locationRepository.Add(locationResult.Value, cancellationToken);
        _logger.LogInformation("Location {@Location} created with id {@Id}", command.Request, id);

        return id;
    }
}