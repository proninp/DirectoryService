using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetLocationsHandler(ILocationRepository locationRepository)
    : IQueryHandler<IReadOnlyList<LocationResponse>, GetLocationsQuery>
{
    public async Task<Result<IReadOnlyList<LocationResponse>, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var locations = await locationRepository.GetAll(cancellationToken);
        if (locations.Count == 0)
        {
            return GeneralError.NotFound(recordName: nameof(Location), message: "No active locations were found.")
                .ToErrors();
        }

        var locationsResponse = locations.ToResponse().ToList();
        return locationsResponse;
    }
}