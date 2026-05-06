using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed partial class GetLocationsHandler(
    ILocationRepository locationRepository,
    ILogger<GetLocationsHandler> logger
)
    : IQueryHandler<IReadOnlyList<LocationResponse>, GetLocationsQuery>
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Found {Count} active locations")]
    private static partial void LogLocationsFound(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No locations found")]
    private static partial void LogNoLocationsFound(ILogger logger);

    public async Task<Result<IReadOnlyList<LocationResponse>, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var locations = await locationRepository.GetAll(cancellationToken);
        if (locations.Count == 0)
        {
            LogNoLocationsFound(logger);
            return GeneralErrors.NotFound(recordName: nameof(Location), message: "No active locations were found.")
                .ToErrors();
        }

        var locationsResponse = locations.ToResponse().ToList();
        LogLocationsFound(logger, locationsResponse.Count);

        return locationsResponse;
    }
}