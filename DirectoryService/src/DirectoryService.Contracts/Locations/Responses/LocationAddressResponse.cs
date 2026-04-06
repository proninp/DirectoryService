namespace DirectoryService.Contracts.Locations.Responses;

public record LocationAddressResponse(
    string PostalCode,
    string Country,
    string City,
    string Street,
    string House,
    string? Block,
    string? Room,
    string? PostalBox
);