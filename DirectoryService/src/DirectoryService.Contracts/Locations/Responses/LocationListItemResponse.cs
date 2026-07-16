namespace DirectoryService.Contracts.Locations.Responses;

public record LocationListItemResponse
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? PostalCode { get; init; }

    public string? Country { get; init; }

    public string? City { get; init; }

    public string? Street { get; init; }

    public string? House { get; init; }

    public string? Block { get; init; }

    public string? Room { get; init; }

    public string? PostalBox { get; init; }

    public DateTime CreatedAt { get; init; }

    public int DepartmentsCount { get; init; }
}