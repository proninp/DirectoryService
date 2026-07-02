using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetLocationHandler(
    IReadDbContext readDbContext,
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

        var locationResponse = readDbContext.LocationsQuery
            .Where(x => x.Id == query.Id)
            .Select(LocationMappingExtensions.ToResponseExpression())
            .FirstOrDefault();

        if (locationResponse is null)
        {
            logger.LogWarning("GetLocation query error: location was not found with id: {LocationId}.", query.Id);
            return GeneralErrors.NotFound(
                    query.Id, nameof(Location), $"No active locations were found with id: '{query.Id}'.")
                .ToErrors();
        }

        return locationResponse;
    }
}