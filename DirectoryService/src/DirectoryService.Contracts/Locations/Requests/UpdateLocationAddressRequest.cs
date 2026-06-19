using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Contracts.Locations.Requests;

public record UpdateLocationAddressRequest(
    string? PostalCode,
    string? Country,
    string? City,
    string? Street,
    string? House,
    string? Block,
    string? Room,
    string? PostalBox
);

public static class UpdateLocationAddressRequestExtensions
{
    public static Result<Address, Errors> ToAddress(this UpdateLocationAddressRequest request)
    {
        return Address.Create(
            request.PostalCode!,
            request.Country!,
            request.City!,
            request.Street!,
            request.House!,
            request.Block,
            request.Room,
            request.PostalBox);
    }

    public static Result<Address, Errors> ToAddress(this UpdateLocationAddressRequest request, Address address)
    {
        return address.With(
            request.PostalCode,
            request.Country,
            request.City,
            request.Street,
            request.House,
            request.Block,
            request.Room,
            request.PostalBox);
    }
}