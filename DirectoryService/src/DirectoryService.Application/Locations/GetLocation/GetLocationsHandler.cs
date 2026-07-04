using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed partial class GetLocationsHandler(
    IReadDbContext context,
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
        var locationsResponse = await context
            .LocationsQuery
            .Select(LocationMappingExtensions.ToResponseExpression())
            .ToListAsync(cancellationToken);
        if (locationsResponse.Count == 0)
        {
            LogNoLocationsFound(logger);
            return GeneralErrors.NotFound(recordName: nameof(Location), message: "No active locations were found.")
                .ToErrors();
        }

        LogLocationsFound(logger, locationsResponse.Count);

        return locationsResponse;
    }
}