namespace DirectoryService.Contracts.Locations.Responses;

public record TopLocationsResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; } = string.Empty;
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? Street { get; init; }
    public string? House { get; init; }
    public string? Block { get; init; }
    public string? Room { get; init; }
    public string? PostalBox { get; init; }
    public int DepartmentsCount { get; init; }
}