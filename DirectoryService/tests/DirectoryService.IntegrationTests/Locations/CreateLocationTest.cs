using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Locations;

#pragma warning disable CA1707
public sealed class CreateLocationTest : DirectoryBaseTest
{
    private static readonly CreateLocationAddressRequest DefaultAddressRequest = new(
        "00-124", "Poland", "Warsaw", "Rondo ONZ", "1", "Tower A", "1205", "PO Box 321");

    public CreateLocationTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocation_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var request = new CreateLocationRequest(
            "Warsaw Technology Center", DefaultAddressRequest, "Europe/Warsaw");

        // Act
        var result = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
        {
            var command = new CreateLocationCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var location = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations.FirstOrDefaultAsync(l => l.Id == result.Value, ct));

        Assert.NotNull(location);
        Assert.Equal("Warsaw Technology Center", location.Name);
        Assert.Equal("Europe/Warsaw", location.Timezone.Value);

        Assert.NotNull(location.Address);
        Assert.Equal("00-124", location.Address.PostalCode);
        Assert.Equal("Poland", location.Address.Country);
        Assert.Equal("Warsaw", location.Address.City);
        Assert.Equal("Rondo ONZ", location.Address.Street);
        Assert.Equal("1", location.Address.House);
        Assert.Equal("Tower A", location.Address.Block);
        Assert.Equal("1205", location.Address.Room);
        Assert.Equal("PO Box 321", location.Address.PostalBox);

        Assert.True(location.IsActive);
        Assert.Null(location.DeletedAt);
        Assert.Null(location.UpdatedAt);
        Assert.True(location.CreatedAt > DateTime.UtcNow.AddMinutes(-1));

        var locationsCount = await ExecuteInDbAsync(dbContext => dbContext.Locations.CountAsync(ct));
        Assert.Equal(1, locationsCount);
    }

    [Fact]
    public async Task CreateLocation_With_Optional_Address_Fields_Null_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var addressRequest = new CreateLocationAddressRequest(
            "00-001", "Poland", "Warsaw", "Marszalkowska", "10", null, null, null);
        var request = new CreateLocationRequest("Marszalkowska Office", addressRequest, "Europe/Warsaw");

        // Act
        var result = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
        {
            var command = new CreateLocationCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);

        var location = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations.FirstOrDefaultAsync(l => l.Id == result.Value, ct));

        Assert.NotNull(location);
        Assert.NotNull(location.Address);
        Assert.Null(location.Address.Block);
        Assert.Null(location.Address.Room);
        Assert.Null(location.Address.PostalBox);
    }

    [Fact]
    public async Task CreateLocation_With_DuplicateAddress_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var firstRequest = new CreateLocationRequest(
            "Warsaw Technology Center", DefaultAddressRequest, "Europe/Warsaw");

        var firstResult = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(firstRequest), ct));
        Assert.True(firstResult.IsSuccess);

        // Act
        var secondRequest = new CreateLocationRequest(
            "Warsaw Technology Center Duplicate", DefaultAddressRequest, "Europe/Warsaw");

        var secondResult = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(secondRequest), ct));

        // Assert
        Assert.True(secondResult.IsFailure);
        var error = Assert.Single(secondResult.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var locationsCount = await ExecuteInDbAsync(dbContext => dbContext.Locations.CountAsync(ct));
        Assert.Equal(1, locationsCount);
    }

    [Fact]
    public async Task CreateLocation_With_DuplicateName_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var firstRequest = new CreateLocationRequest(
            "Warsaw Technology Center", DefaultAddressRequest, "Europe/Warsaw");

        var firstResult = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(firstRequest), ct));
        Assert.True(firstResult.IsSuccess);

        // Act
        var otherAddressRequest = new CreateLocationAddressRequest(
            "00-002", "Poland", "Krakow", "Rynek Glowny", "2", null, null, null);
        var secondRequest = new CreateLocationRequest(
            "Warsaw Technology Center", otherAddressRequest, "Europe/Warsaw");

        var secondResult = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(secondRequest), ct));

        // Assert
        Assert.True(secondResult.IsFailure);
        var error = Assert.Single(secondResult.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var locationsCount = await ExecuteInDbAsync(dbContext => dbContext.Locations.CountAsync(ct));
        Assert.Equal(1, locationsCount);
    }

    [Fact]
    public async Task CreateLocation_With_TooShort_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var request = new CreateLocationRequest("AB", DefaultAddressRequest, "Europe/Warsaw");

        // Act
        var result = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(request), ct));

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var locationsCount = await ExecuteInDbAsync(dbContext => dbContext.Locations.CountAsync(ct));
        Assert.Equal(0, locationsCount);
    }

    [Fact]
    public async Task CreateLocation_With_TooLong_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var tooLongName = new string('A', Location.NameMaxLength + 1);
        var request = new CreateLocationRequest(tooLongName, DefaultAddressRequest, "Europe/Warsaw");

        // Act
        var result = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(request), ct));

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var locationsCount = await ExecuteInDbAsync(dbContext => dbContext.Locations.CountAsync(ct));
        Assert.Equal(0, locationsCount);
    }
}