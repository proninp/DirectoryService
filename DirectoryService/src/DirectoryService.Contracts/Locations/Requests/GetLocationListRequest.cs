using DirectoryService.Contracts.Pagination;

namespace DirectoryService.Contracts.Locations.Requests;

public record GetLocationListRequest(
    string? Search,
    int MinDepartmentCount,
    string? SortBy,
    string? SortDir,
    PagedRequest Pagination);