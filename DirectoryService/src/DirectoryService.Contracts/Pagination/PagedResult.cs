namespace DirectoryService.Contracts.Pagination;

public record PagedResult<T>
{
    public long TotalCount { get; init; }

    public required IReadOnlyList<T> Items { get; init; }
}