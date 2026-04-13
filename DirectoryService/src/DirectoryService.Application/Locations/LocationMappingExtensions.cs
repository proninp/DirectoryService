using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public static class LocationMappingExtensions
{
    public static Result<Address, Errors> ToAddress(this CreateLocationAddressRequest addressRequest)
    {
        return Address.Create(
            addressRequest.PostalCode,
            addressRequest.Country,
            addressRequest.City,
            addressRequest.Street,
            addressRequest.House,
            addressRequest.Block,
            addressRequest.Room,
            addressRequest.PostalBox
        );
    }

    public static LocationResponse ToResponse(this Location location)
    {
        return new LocationResponse(
            location.Id,
            location.Name,
            location.Address?.ToAddressResponse(),
            location.Timezone.ToString()
        );
    }

    private static LocationAddressResponse ToAddressResponse(this Address address)
    {
        return new LocationAddressResponse(
            address.PostalCode,
            address.Country,
            address.City,
            address.Street,
            address.House,
            address.Block,
            address.Room,
            address.PostalBox
        );
    }
}