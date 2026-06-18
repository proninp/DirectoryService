namespace DirectoryService.Contracts.Locations.Requests;

public record UpdateLocationRequest(
    string Name,
    UpdateLocationAddressRequest AddressRequest,
    string Timezone
);