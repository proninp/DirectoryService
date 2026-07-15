using System.Data;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Contracts.Pagination;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetLocationListHandler(
    IValidator<GetLocationListQuery> validator,
    IDbConnectionFactory connectionFactory,
    ILogger<GetLocationListHandler> logger
) : IQueryHandler<PagedResult<LocationListItemResponse>, GetLocationListQuery>
{
    public async Task<Result<PagedResult<LocationListItemResponse>, Errors>> Handle(
        GetLocationListQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            logger.LogWarning(
                "Validation failed for {Request}: {@Context} with errors: {@Errors}",
                nameof(query.Request),
                query.Request,
                errors);
            return errors;
        }

        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        if (string.IsNullOrEmpty(query.Request.Search))
        {
            conditions.Add("l.name ilike @search");
            parameters.Add("search", $"%{query.Request.Search}%", DbType.String);
        }

        if (query.Request.MinDepartmentCount.HasValue)
        {
            conditions.Add("coalesce(ld.departments_count, 0) >= @minDepartmentCount");
            parameters.Add("minDepartmentCount", query.Request.MinDepartmentCount.Value, DbType.Int32);
        }

        parameters.Add("offset", (query.Request.Pagination.PageNumber - 1) * query.Request.Pagination.PageSize);
        parameters.Add("page_size", query.Request.Pagination.PageSize);

        var whereClause = conditions.Count > 0
            ? string.Concat("where ", string.Join(" and ", conditions))
            : string.Empty;

        var sortBy = "l.id";
        if (!string.IsNullOrEmpty(query.Request.SortBy))
        {
            if (string.Equals(nameof(Location.Name), query.Request.SortBy, StringComparison.OrdinalIgnoreCase))
                sortBy = "l.name";
            else if (string.Equals(nameof(Location.CreatedAt), query.Request.SortBy,
                         StringComparison.OrdinalIgnoreCase))
                sortBy = "l.created_at";
            else if (string.Equals(nameof(GetLocationListQuery.Request.MinDepartmentCount), query.Request.SortBy,
                         StringComparison.OrdinalIgnoreCase))
                sortBy = "dl.departments_count";
        }

        var sortDir = string.Equals(query.Request.SortDir, "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";

        var orderByClause = $"order by {sortBy} {sortDir}";

        var sql = $"""
                   with ld as (
                       select dl.location_id,
                              count(dl.department_id) departments_count
                       from department_locations dl
                       group by dl.location_id
                   )
                   select l.id,
                          l.name,
                          l.postal_code,
                          l.country,
                          l.city,
                          l.street,
                          l.house,
                          l.block,
                          l.room,
                          l.postal_box,
                          l.created_at,
                          coalesce(ld.departments_count, 0),
                          count(*) over () total_count
                   from locations l 
                   left join ld on ld.location_id = l.id
                   {whereClause}
                   {orderByClause}
                   limit @page_size offset @offset
                   """;

        using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

        var locations = await connection.QueryAsync<LocationListItemResponse>(
            sql, param: parameters);

        throw new NotImplementedException();
    }
}