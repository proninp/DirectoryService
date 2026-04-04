using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Application.Locations;

public sealed class CreateLocationHandler : ICreateLocationHandler
{
    private readonly ILocationRepository _locationRepository;

    public CreateLocationHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Result<Guid>> Handle(
        CreateLocationRequest request,
        CancellationToken cancellationToken
    )
    {
        var addressResult = request.AddressRequest.ToAddress();
        if (addressResult.IsFailure)
        {
            return Result.Failure<Guid>(addressResult.Error);
        }

        var location = Location.Create(
            request.Name, addressResult.Value, new Timezone(request.Timezone)
        );

        if (location.IsFailure)
        {
            return Result.Failure<Guid>(location.Error);
        }

        var id = await _locationRepository.Create(location.Value, cancellationToken);

        return Result.Success(id);
    }
}