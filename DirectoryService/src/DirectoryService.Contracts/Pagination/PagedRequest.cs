namespace DirectoryService.Contracts.Pagination;

public record PagedRequest(int PageNumber = 1, int PageSize = 20)
{
    public const int MinPageSize = 1;

    public const int MaxPageSize = 100;
}