using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Application.Locations;

public static class CreateLocationRequestExtensions
{
    public static Result<Address> ToAddress(this CreateLocationRequest request)
    {
        return Address.Create(
            request.PostalCode,
            request.Country,
            request.City,
            request.Street,
            request.House,
            request.Block,
            request.Room,
            request.PostalBox
        );
    }
}