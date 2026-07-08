namespace DirectoryService.Contracts.Pagination;

public record PagedRequest(int PageNumber = 1, int PageSize = 20);