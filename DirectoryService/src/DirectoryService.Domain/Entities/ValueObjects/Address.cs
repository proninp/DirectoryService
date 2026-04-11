using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Address
{
    private const int PostalCodeMinLength = 2;

    public const int PostalCodeMaxLength = 10;

    private const int CountryMinLength = 2;

    public const int CountryMaxLength = 50;

    private const int CityMinLength = 3;

    public const int CityMaxLength = 50;

    private const int StreetMinLength = 3;

    public const int StreetMaxLength = 100;

    private const int HouseMinLength = 1;

    public const int HouseMaxLength = 50;

    private const int BlockMinLength = 1;

    public const int BlockMaxLength = 10;

    private const int RoomMinLength = 1;

    public const int RoomMaxLength = 10;

    private const int PostalBoxMinLength = 1;

    public const int PostalBoxMaxLength = 20;

    public string PostalCode { get; } = null!;

    public string Country { get; } = null!;

    public string City { get; } = null!;

    public string Street { get; } = null!;

    public string House { get; } = null!;

    public string? Block { get; }

    public string? Room { get; }

    public string? PostalBox { get; }

    // EF Core Constructor
    private Address() { }

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

    public static Result<Address, Error> Create(string postalCode, string country, string city, string street, string house,
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
                ? UnitResult.Success<Error>()
                : Guard.ValidateStringField(block, nameof(block), BlockMinLength, BlockMaxLength),
            string.IsNullOrEmpty(room)
                ? UnitResult.Success<Error>()
                : Guard.ValidateStringField(room, nameof(room), RoomMinLength, RoomMaxLength),
            string.IsNullOrEmpty(postalBox)
                ? UnitResult.Success<Error>()
                : Guard.ValidateStringField(postalBox, nameof(postalBox), PostalBoxMinLength, PostalBoxMaxLength)
        );

        if (validationResult.IsFailure)
            return validationResult.Error;

        return new Address(postalCode, country, city, street, house, block, room, postalBox);
    }
}