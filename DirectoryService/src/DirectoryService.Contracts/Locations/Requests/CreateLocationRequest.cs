namespace DirectoryService.Contracts.Locations.Requests;

public sealed record CreateLocationRequest(
    string Name,
    string PostalCode,
    string Country,
    string City,
    string Street,
    string House,
    string? Block,
    string? Room,
    string? PostalBox,
    string Timezone
);