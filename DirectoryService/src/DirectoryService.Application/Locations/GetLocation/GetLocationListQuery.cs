using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Locations.GetLocation;

public record GetLocationListQuery(GetLocationListRequest Request) : IQuery
{
    public static readonly Dictionary<string, string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(Location.Name)] = "l.name",
        [nameof(Location.CreatedAt)] = "l.created_at",
        [nameof(GetLocationListQuery.Request.DepartmentsCount)] = "departments_count",
    };
}

public static class GetLocationListQueryExtensions
{
    public static GetLocationListQuery ToQuery(this GetLocationListRequest request) =>
        new(request);
}