using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public sealed class GetLocationHandler(ILocationRepository locationRepository) : IGetLocationHandler
{
    public async Task<Result<LocationResponse, Errors>> Handle(
        Guid id,
        CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetById(id, cancellationToken);
        if (location is null)
            return GeneralError.NotFound(id, nameof(Location)).ToErrors();

        return location.ToResponse();
    }
}