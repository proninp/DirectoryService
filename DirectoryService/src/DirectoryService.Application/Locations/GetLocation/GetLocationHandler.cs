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
) : IQueryHandler<LocationResponse, GetLocationQuery>
{
    public async Task<Result<LocationResponse, Errors>> Handle(
        GetLocationQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Id == Guid.Empty)
        {
            logger.LogWarning("Get Location query error: query id parameter is empty.");
            return GeneralErrors.ValueIsRequired(nameof(query.Id)).ToErrors();
        }

        var location = await locationRepository.GetById(query.Id, cancellationToken);
        if (location is null)
        {
            logger.LogWarning("GetLocation query error: location was not found with id: {LocationId}.", query.Id);
            return GeneralErrors.NotFound(
                    query.Id, nameof(Location), $"No active locations were found with id: '{query.Id}'.")
                .ToErrors();
        }

        return location.ToResponse();
    }
}