using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Address
{
    private const int PostalCodeMinLength = 2;

    private const int PostalCodeMaxLength = 10;

    private const int CountryMinLength = 2;

    private const int CountryMaxLength = 50;

    private const int CityMinLength = 3;

    private const int CityMaxLength = 50;

    private const int StreetMinLength = 3;

    private const int StreetMaxLength = 100;

    private const int HouseMinLength = 1;

    private const int HouseMaxLength = 50;

    private const int BlockMinLength = 1;

    private const int BlockMaxLength = 10;

    private const int RoomMinLength = 1;

    private const int RoomMaxLength = 10;

    private const int PostalBoxMinLength = 1;

    private const int PostalBoxMaxLength = 20;

    public string PostalCode { get; }

    public string Country { get; }

    public string City { get; }

    public string Street { get; }

    public string House { get; }

    public string? Block { get; }

    public string? Room { get; }

    public string? PostalBox { get; }

    private Address(string postalCode, string country, string city, string street, string house,
        string? block = null, string? room = null, string? postalBox = null
    )
    {
        PostalCode = postalCode;
        Country = country;
        City = city;
        Street = street;
        House = house;
        Block = block;
        Room = room;
        PostalBox = postalBox;
    }

    public static Result<Address> Create(string postalCode, string country, string city, string street, string house,
        string? block = null, string? room = null, string? postalBox = null
    )
    {
        var validationResult = Result.Combine(
            Guard.ValidateStringField(postalCode, nameof(postalCode), PostalCodeMinLength, PostalCodeMaxLength),
            Guard.ValidateStringField(country, nameof(country), CountryMinLength, CountryMaxLength),
            Guard.ValidateStringField(city, nameof(city), CityMinLength, CityMaxLength),
            Guard.ValidateStringField(street, nameof(street), StreetMinLength, StreetMaxLength),
            Guard.ValidateStringField(house, nameof(house), HouseMinLength, HouseMaxLength),
            string.IsNullOrEmpty(block)
                ? Result.Success()
                : Guard.ValidateStringField(block, nameof(block), BlockMinLength, BlockMaxLength),
            string.IsNullOrEmpty(room)
                ? Result.Success()
                : Guard.ValidateStringField(room, nameof(room), RoomMinLength, RoomMaxLength),
            string.IsNullOrEmpty(postalBox)
                ? Result.Success()
                : Guard.ValidateStringField(postalBox, nameof(postalBox), PostalBoxMinLength, PostalBoxMaxLength)
        );

        if (validationResult.IsFailure)
        {
            return Result.Failure<Address>(validationResult.Error);
        }

        return new Address(postalCode, country, city, street, house, block, room, postalBox);
    }
}