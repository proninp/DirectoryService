namespace DirectoryService.Contracts.Locations.Requests;

public record UpdateLocationAddressRequest(
    string PostalCode,
    string Country,
    string City,
    string Street,
    string House,
    string? Block,
    string? Room,
    string? PostalBox
);