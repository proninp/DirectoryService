using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.CreateLocation;

public sealed class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;

    public CreateLocationHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var addressResult = command.Request.AddressRequest.ToAddress();
        if (addressResult.IsFailure)
            return addressResult.Error;

        var locationResult = Location.Create(
            command.Request.Name,
            addressResult.Value,
            new Timezone(command.Request.Timezone)
        );

        if (locationResult.IsFailure)
            return locationResult.Error;

        var id = await _locationRepository.Add(locationResult.Value, cancellationToken);

        return id;
    }
}