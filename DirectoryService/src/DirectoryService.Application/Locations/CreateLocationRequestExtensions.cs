using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public static class CreateLocationRequestExtensions
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
}