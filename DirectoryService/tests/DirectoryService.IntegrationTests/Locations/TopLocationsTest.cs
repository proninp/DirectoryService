using System.Globalization;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.GetLocation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Locations;

#pragma warning disable CA1707
public sealed class TopLocationsTest : DirectoryBaseTest
{
    public TopLocationsTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTopLocations_With_No_Locations_Should_ReturnEmpty()
    {
        // Arrange
        var ct = CancellationToken.None;

        // Act
        var result = await ExecuteHandlerAsync((GetTopLocationsQueryHandler sut) =>
            sut.Handle(new GetLocationsQuery(), ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetTopLocations_With_Locations_Without_Departments_Should_Return_Locations()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId1 = await CreateLocationAsync("Warsaw Office", "001", ct);
        var locationId2 = await CreateLocationAsync("Krakow Office", "002", ct);

        // Act
        var result = await ExecuteHandlerAsync((GetTopLocationsQueryHandler sut) =>
            sut.Handle(new GetLocationsQuery(), ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, l => Assert.Equal(0, l.DepartmentsCount));
        Assert.Contains(result.Value, l => l.Id == locationId1);
        Assert.Contains(result.Value, l => l.Id == locationId2);
    }

    [Fact]
    public async Task GetTopLocations_Should_OrderByDepartmentsCount_Descending()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationWithNoDepartments = await CreateLocationAsync("No Departments Office", "001", ct);
        var locationWithOneDepartment = await CreateLocationAsync("One Department Office", "002", ct);
        var locationWithTwoDepartments = await CreateLocationAsync("Two Departments Office", "003", ct);

        await LinkDepartmentAsync(locationWithOneDepartment, "DEPTA", ct);
        await LinkDepartmentAsync(locationWithTwoDepartments, "DEPTB", ct);
        await LinkDepartmentAsync(locationWithTwoDepartments, "DEPTC", ct);

        // Act
        var result = await ExecuteHandlerAsync((GetTopLocationsQueryHandler sut) =>
            sut.Handle(new GetLocationsQuery(), ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(locationWithTwoDepartments, result.Value[0].Id);
        Assert.Equal(2, result.Value[0].DepartmentsCount);
        Assert.Equal(locationWithOneDepartment, result.Value[1].Id);
        Assert.Equal(1, result.Value[1].DepartmentsCount);
        Assert.Equal(locationWithNoDepartments, result.Value[2].Id);
        Assert.Equal(0, result.Value[2].DepartmentsCount);
    }

    [Fact]
    public async Task GetTopLocations_Should_Exclude_Inactive_Departments_From_Count()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync("Mixed Departments Office", "001", ct);

        await LinkDepartmentAsync(locationId, "DEPTA", ct, departmentIsActive: true);
        await LinkDepartmentAsync(locationId, "DEPTB", ct, departmentIsActive: false);

        // Act
        var result = await ExecuteHandlerAsync((GetTopLocationsQueryHandler sut) =>
            sut.Handle(new GetLocationsQuery(), ct));

        // Assert
        Assert.True(result.IsSuccess);
        var location = Assert.Single(result.Value);
        Assert.Equal(locationId, location.Id);
        Assert.Equal(1, location.DepartmentsCount);
    }

    [Fact]
    public async Task GetTopLocations_Should_Exclude_Inactive_Locations()
    {
        // Arrange
        var ct = CancellationToken.None;
        var activeLocationId = await CreateLocationAsync("Active Office", "001", ct);
        var inactiveLocationId = await CreateLocationAsync("Inactive Office", "002", ct);
        await DeactivateLocationAsync(inactiveLocationId, ct);

        // Act
        var result = await ExecuteHandlerAsync((GetTopLocationsQueryHandler sut) =>
            sut.Handle(new GetLocationsQuery(), ct));

        // Assert
        Assert.True(result.IsSuccess);
        var location = Assert.Single(result.Value);
        Assert.Equal(activeLocationId, location.Id);
    }

    [Fact]
    public async Task GetTopLocations_Should_Limit_To_Five()
    {
        // Arrange
        var ct = CancellationToken.None;
        for (var i = 1; i <= 6; i++)
            await CreateLocationAsync($"Office {i}", i.ToString("D3", CultureInfo.InvariantCulture), ct);

        // Act
        var result = await ExecuteHandlerAsync((GetTopLocationsQueryHandler sut) =>
            sut.Handle(new GetLocationsQuery(), ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Count);
    }

    [Fact]
    public async Task GetTopLocations_With_One_Location_Having_Departments_Should_Return_It_First_Then_OrderByCreatedAt_Descending()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId1 = await CreateLocationAsync("Office 1", "001", ct);
        var locationId2 = await CreateLocationAsync("Office 2", "002", ct);
        var locationId3 = await CreateLocationAsync("Office 3", "003", ct);
        var locationId4 = await CreateLocationAsync("Office 4", "004", ct);
        var locationId5 = await CreateLocationAsync("Office 5", "005", ct);
        var locationId6 = await CreateLocationAsync("Office 6", "006", ct);

        // locationId2 is not the most recently created location, so if the query ordered
        // purely by created_at, it would not naturally end up first.
        await LinkDepartmentAsync(locationId2, "DEPTA", ct);
        await LinkDepartmentAsync(locationId2, "DEPTB", ct);

        // Act
        var result = await ExecuteHandlerAsync((GetTopLocationsQueryHandler sut) =>
            sut.Handle(new GetLocationsQuery(), ct));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Count);

        Assert.Equal(locationId2, result.Value[0].Id);
        Assert.Equal(2, result.Value[0].DepartmentsCount);

        var remaining = result.Value.Skip(1).Select(l => l.Id).ToList();
        Assert.Equal([locationId6, locationId5, locationId4, locationId3], remaining);
        Assert.All(result.Value.Skip(1), l => Assert.Equal(0, l.DepartmentsCount));

        // locationId1 is the oldest location without departments, so it is the one dropped by LIMIT 5.
        Assert.DoesNotContain(result.Value, l => l.Id == locationId1);
    }

    private async Task<Guid> CreateLocationAsync(string name, string addressSuffix, CancellationToken ct)
    {
        var addressRequest = new CreateLocationAddressRequest(
            $"00-{addressSuffix}", "Poland", "Warsaw", $"Street {addressSuffix}", "1", null, null, null);
        var request = new CreateLocationRequest(name, addressRequest, "Europe/Warsaw");

        var result = await ExecuteHandlerAsync((CreateLocationHandler sut) =>
            sut.Handle(new CreateLocationCommand(request), ct));

        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private async Task LinkDepartmentAsync(
        Guid locationId, string slug, CancellationToken ct, bool departmentIsActive = true)
    {
        await ExecuteInDbAsync(async dbContext =>
        {
            var departmentId = Guid.NewGuid();
            var departmentSlug = Slug.Create(slug).Value;
            var departmentLocation = new DepartmentLocation(departmentId, locationId);

            var department = Department.Create(
                    $"Department {slug}", departmentSlug, [departmentLocation], departmentId: departmentId)
                .Value;

            if (!departmentIsActive)
                department.Delete();

            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(ct);
            return department.Id;
        });
    }

    private async Task DeactivateLocationAsync(Guid locationId, CancellationToken ct)
    {
        await ExecuteInDbAsync(async dbContext =>
        {
            var location = await dbContext.Locations.FirstAsync(l => l.Id == locationId, ct);
            location.Delete();
            await dbContext.SaveChangesAsync(ct);
            return true;
        });
    }
}