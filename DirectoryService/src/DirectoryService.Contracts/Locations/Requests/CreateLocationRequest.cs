namespace DirectoryService.Contracts.Locations.Requests;

public sealed record CreateLocationRequest(
    string Name,
    CreateLocationAddressRequest AddressRequest,
    string Timezone
);