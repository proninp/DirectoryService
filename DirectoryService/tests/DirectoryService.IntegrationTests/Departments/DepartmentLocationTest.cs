using DirectoryService.Application.Departments.CreateDepartmentLocation;
using DirectoryService.Application.Departments.DeleteDepartmentLocation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

#pragma warning disable CA1707
public sealed class DepartmentLocationTest : DirectoryBaseTest
{
    public DepartmentLocationTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartmentLocation_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId1 = await CreateLocationAsync(ct);
        var departmentId = await CreateDepartmentAsync([locationId1], ct);
        var locationId2 = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentLocationHandler sut) =>
        {
            var command = new CreateDepartmentLocationCommand(departmentId, locationId2);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId, result.Value.Id);
        Assert.Equal(2, result.Value.LocationIds.Count);
        Assert.Contains(locationId1, result.Value.LocationIds);
        Assert.Contains(locationId2, result.Value.LocationIds);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        Assert.Equal(2, department.DepartmentLocations.Count);
        Assert.Contains(department.DepartmentLocations, dl => dl.LocationId == locationId2);
    }

    [Fact]
    public async Task CreateDepartmentLocation_With_NonExistingDepartment_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);
        var nonExistingDepartmentId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentLocationHandler sut) =>
        {
            var command = new CreateDepartmentLocationCommand(nonExistingDepartmentId, locationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task CreateDepartmentLocation_With_NonExistingLocation_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);
        var departmentId = await CreateDepartmentAsync([locationId], ct);
        var nonExistingLocationId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentLocationHandler sut) =>
        {
            var command = new CreateDepartmentLocationCommand(departmentId, nonExistingLocationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        Assert.Single(department.DepartmentLocations);
    }

    [Fact]
    public async Task CreateDepartmentLocation_With_ExistingRelation_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);
        var departmentId = await CreateDepartmentAsync([locationId], ct);

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentLocationHandler sut) =>
        {
            var command = new CreateDepartmentLocationCommand(departmentId, locationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);
    }

    [Fact]
    public async Task DeleteDepartmentLocation_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId1 = await CreateLocationAsync(ct);
        var locationId2 = await CreateLocationAsync(ct);
        var departmentId = await CreateDepartmentAsync([locationId1, locationId2], ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentLocationHandler sut) =>
        {
            var command = new DeleteDepartmentLocationCommand(departmentId, locationId2);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId, result.Value.Id);
        var remainingLocationId = Assert.Single(result.Value.LocationIds);
        Assert.Equal(locationId1, remainingLocationId);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        var location = Assert.Single(department.DepartmentLocations);
        Assert.Equal(locationId1, location.LocationId);
    }

    [Fact]
    public async Task DeleteDepartmentLocation_With_NonExistingDepartment_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);
        var nonExistingDepartmentId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentLocationHandler sut) =>
        {
            var command = new DeleteDepartmentLocationCommand(nonExistingDepartmentId, locationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task DeleteDepartmentLocation_With_NonExistingRelation_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);
        var departmentId = await CreateDepartmentAsync([locationId], ct);
        var otherLocationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentLocationHandler sut) =>
        {
            var command = new DeleteDepartmentLocationCommand(departmentId, otherLocationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task DeleteDepartmentLocation_With_OnlyLocation_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);
        var departmentId = await CreateDepartmentAsync([locationId], ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentLocationHandler sut) =>
        {
            var command = new DeleteDepartmentLocationCommand(departmentId, locationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.LocationIds);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        Assert.Empty(department.DepartmentLocations);
    }

    private async Task<Guid> CreateLocationAsync(CancellationToken ct)
    {
        var locationId = await ExecuteInDbAsync(async dbContext =>
        {
            var locationAddress = Address.Create(
                "00-124", "Poland", "Warsaw", "Rondo ONZ",
                Guid.NewGuid().ToString("N")[..8], "Tower A", "1205", "PO Box 321");
            var locationTimezone = Timezone.Create("Europe/Warsaw");

            var location = Location.Create(
                    $"Warsaw Technology Center {Guid.NewGuid()}", locationAddress.Value, locationTimezone.Value)
                .Value;

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync(ct);
            return location.Id;
        });
        return locationId;
    }

    private async Task<Guid> CreateDepartmentAsync(IReadOnlyCollection<Guid> locationIds, CancellationToken ct)
    {
        var departmentId = await ExecuteInDbAsync(async dbContext =>
        {
            var newDepartmentId = Guid.NewGuid();
            var slug = Slug.Create("DEPTLOC").Value;
            var departmentLocations = locationIds
                .Select(locationId => new DepartmentLocation(newDepartmentId, locationId))
                .ToList();

            var department = Department.Create(
                    "Department With Locations", slug, departmentLocations, departmentId: newDepartmentId)
                .Value;

            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(ct);
            return department.Id;
        });

        return departmentId;
    }
}