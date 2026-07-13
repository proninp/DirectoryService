using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.GetLocation;

public record GetLocationListQuery(GetLocationListRequest Request) : IQuery;

public static class GetLocationListQueryExtensions
{
    public static GetLocationListQuery ToQuery(this GetLocationListRequest request) =>
        new(request);
}