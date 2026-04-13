namespace DirectoryService.Contracts.Locations.Responses;

public record LocationResponse(
    Guid Id,
    string Name,
    LocationAddressResponse? AddressResponse,
    string Timezone
);