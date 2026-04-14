using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetLocationHandler(ILocationRepository locationRepository)
    : IQueryHandler<LocationResponse, GetLocationQuery>
{
    public async Task<Result<LocationResponse, Errors>> Handle(
        GetLocationQuery query,
        CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetById(query.Id, cancellationToken);
        if (location is null)
            return GeneralError.NotFound(query.Id, nameof(Location)).ToErrors();

        return location.ToResponse();
    }
}