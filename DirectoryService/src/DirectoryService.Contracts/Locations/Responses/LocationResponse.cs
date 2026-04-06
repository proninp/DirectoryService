namespace DirectoryService.Contracts.Locations.Responses;

public record LocationResponse(
    string Name,
    LocationAddressResponse AddressResponse,
    string Timezone
);