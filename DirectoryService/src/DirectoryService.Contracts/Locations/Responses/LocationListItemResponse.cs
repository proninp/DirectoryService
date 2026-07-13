namespace DirectoryService.Contracts.Locations.Responses;

public record LocationListItemResponse(
    Guid Id,
    string Name,
    string PostalCode,
    string Country,
    string City,
    string Street,
    string House,
    string? Block,
    string? Room,
    string? PostalBox,
    DateTime CreatedAt,
    int DepartmentCount);