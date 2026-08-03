using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.UpdateLocation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Locations;

#pragma warning disable CA1707
public sealed class UpdateLocationTest : DirectoryBaseTest
{
    private static readonly CreateLocationAddressRequest DefaultAddressRequest = new(
        "00-124", "Poland", "Warsaw", "Rondo ONZ", "1", "Tower A", "1205", "PO Box 321");

    public UpdateLocationTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateLocation_With_Valid_Name_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(
                locationId, new UpdateLocationRequest("Warsaw Technology Center Renamed", null, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Warsaw Technology Center Renamed", result.Value.Name);

        var location = await GetLocationAsync(locationId, ct);
        Assert.Equal("Warsaw Technology Center Renamed", location.Name);
        Assert.Equal("Europe/Warsaw", location.Timezone.Value);
        Assert.NotNull(location.UpdatedAt);
    }

    [Fact]
    public async Task UpdateLocation_With_Same_Name_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(
                locationId, new UpdateLocationRequest("Warsaw Technology Center", null, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Warsaw Technology Center", result.Value.Name);
    }

    [Fact]
    public async Task UpdateLocation_With_Valid_Address_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var addressRequest = new UpdateLocationAddressRequest(
                null, null, "Krakow", "Rynek Glowny", null, null, null, null);
            var command = new UpdateLocationCommand(
                locationId, new UpdateLocationRequest(null, addressRequest, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.AddressResponse);
        Assert.Equal("Krakow", result.Value.AddressResponse.City);
        Assert.Equal("Rynek Glowny", result.Value.AddressResponse.Street);
        Assert.Equal("00-124", result.Value.AddressResponse.PostalCode);

        var location = await GetLocationAsync(locationId, ct);
        Assert.NotNull(location.Address);
        Assert.Equal("Krakow", location.Address.City);
        Assert.Equal("Rynek Glowny", location.Address.Street);
        Assert.Equal("00-124", location.Address.PostalCode);
        Assert.Equal("Poland", location.Address.Country);
        Assert.Equal("1", location.Address.House);
        Assert.Equal("Tower A", location.Address.Block);
    }

    [Fact]
    public async Task UpdateLocation_With_Valid_Timezone_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(
                locationId, new UpdateLocationRequest(null, null, "Europe/Berlin"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Europe/Berlin", result.Value.Timezone);

        var location = await GetLocationAsync(locationId, ct);
        Assert.Equal("Europe/Berlin", location.Timezone.Value);
    }

    [Fact]
    public async Task UpdateLocation_With_NonExistingLocation_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingLocationId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(
                nonExistingLocationId, new UpdateLocationRequest("New Name", null, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task UpdateLocation_With_Empty_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(
                locationId, new UpdateLocationRequest(string.Empty, null, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var location = await GetLocationAsync(locationId, ct);
        Assert.Equal("Warsaw Technology Center", location.Name);
    }

    [Fact]
    public async Task UpdateLocation_With_TooShort_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(locationId, new UpdateLocationRequest("AB", null, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var location = await GetLocationAsync(locationId, ct);
        Assert.Equal("Warsaw Technology Center", location.Name);
    }

    [Fact]
    public async Task UpdateLocation_With_TooLong_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);
        var tooLongName = new string('A', Location.NameMaxLength + 1);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(locationId, new UpdateLocationRequest(tooLongName, null, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var location = await GetLocationAsync(locationId, ct);
        Assert.Equal("Warsaw Technology Center", location.Name);
    }

    [Fact]
    public async Task UpdateLocation_With_Invalid_Address_Field_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var addressRequest = new UpdateLocationAddressRequest(
                null, null, "AB", null, null, null, null, null);
            var command = new UpdateLocationCommand(
                locationId, new UpdateLocationRequest(null, addressRequest, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var location = await GetLocationAsync(locationId, ct);
        Assert.NotNull(location.Address);
        Assert.Equal("Warsaw", location.Address.City);
    }

    [Fact]
    public async Task UpdateLocation_With_Invalid_Timezone_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateLocationHandler sut) =>
        {
            var command = new UpdateLocationCommand(
                locationId, new UpdateLocationRequest(null, null, "Not/AValidZone"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var location = await GetLocationAsync(locationId, ct);
        Assert.Equal("Europe/Warsaw", location.Timezone.Value);
    }

    private async Task<Guid> CreateLocationAsync(CancellationToken ct)
    {
        var request = new CreateLocationRequest(
            "Warsaw Technology Center", DefaultAddressRequest, "Europe/Warsaw");

        var result = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(request), ct));

        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private async Task<Location> GetLocationAsync(Guid locationId, CancellationToken ct)
    {
        var location = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations.FirstOrDefaultAsync(l => l.Id == locationId, ct));

        Assert.NotNull(location);
        return location;
    }
}