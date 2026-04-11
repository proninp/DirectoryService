using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public sealed class CreateLocationHandler : ICreateLocationHandler
{
    private readonly ILocationRepository _locationRepository;

    public CreateLocationHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<Guid, Error>> Handle(
        CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var addressResult = request.AddressRequest.ToAddress();
        if (addressResult.IsFailure)
            return addressResult.Error;

        var locationResult = Location.Create(
            request.Name,
            addressResult.Value,
            new Timezone(request.Timezone)
        );

        if (locationResult.IsFailure)
            return locationResult.Error;

        var id = await _locationRepository.Add(locationResult.Value, cancellationToken);

        return id;
    }
}