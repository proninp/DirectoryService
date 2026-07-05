using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetTopLocationsQueryHandler :
    IQueryHandler<IReadOnlyList<TopLocationsResponse>, GetLocationsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<GetTopLocationsQueryHandler> _logger;

    public GetTopLocationsQueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<GetTopLocationsQueryHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TopLocationsResponse>, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var sql = """
                  WITH locationsWithDepartments AS (
                      SELECT l.id
                           , count(dl.department_id) departments_count
                      FROM locations l
                          JOIN department_locations dl on dl.location_id = l.id
                          LEFT JOIN departments d on d.id = dl.department_id
                      WHERE l.is_active = true and d.is_active = true
                      GROUP BY l.id)
                  SELECT l.id,
                         l.name,
                         l.postal_code,
                         l.country,
                         l.city,
                         l.street,
                         l.house,
                         l.block,
                         l.room,
                         l.postal_box,
                         ld.departments_count
                  FROM locations l
                  JOIN locationsWithDepartments ld ON ld.id = l.id
                  ORDER BY departments_count DESC, l.name
                  LIMIT 5;
                  """;

        var topLocationsResponses =
            await connection.QueryAsync<TopLocationsResponse>(
                sql, cancellationToken);

        var result = topLocationsResponses.ToList().AsReadOnly();

        return result;
    }
}