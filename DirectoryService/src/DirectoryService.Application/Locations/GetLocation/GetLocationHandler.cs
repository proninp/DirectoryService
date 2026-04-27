using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetLocationHandler(
    ILocationRepository locationRepository,
    ILogger<GetLocationHandler> logger
)
    : IQueryHandler<LocationResponse, GetLocationQuery>
{
    public async Task<Result<LocationResponse, Errors>> Handle(
        GetLocationQuery query,
        CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetById(query.Id, cancellationToken);
        if (location is null)
        {
            logger.LogError("Location with id {LocationId} not found", query.Id);
            return GeneralError.NotFound(query.Id, nameof(Location)).ToErrors();
        }

        logger.LogInformation("Found the location with id {LocationId}", query.Id);
        return location.ToResponse();
    }
}