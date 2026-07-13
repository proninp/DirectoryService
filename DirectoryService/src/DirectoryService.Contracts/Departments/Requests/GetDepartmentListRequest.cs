using DirectoryService.Contracts.Pagination;

namespace DirectoryService.Contracts.Departments.Requests;

public record GetDepartmentListRequest(
    string? Search,
    string? SortBy,
    string? SortDir,
    PagedRequest Pagination
);