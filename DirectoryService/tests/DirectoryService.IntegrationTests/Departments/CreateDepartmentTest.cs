using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

#pragma warning disable CA1707
public class CreateDepartmentTest : DepartmentBaseTest
{
    public CreateDepartmentTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocation(ct);

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Enterprise Architecture", "ENTARCH", null, [locationId]));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(d => d.Id == result.Value, ct);
        });

        Assert.NotNull(department);
        Assert.Equal("Enterprise Architecture", department.Name);
        Assert.Equal("ENTARCH", department.Slug.Value);
        Assert.Null(department.ParentId);
        Assert.Equal(0, department.Depth);
        Assert.Equal("ENTARCH", department.Path.Value);

        Assert.True(department.IsActive);
        Assert.Null(department.DeletedAt);
        Assert.Null(department.UpdatedAt);
        Assert.True(department.CreatedAt > DateTime.UtcNow.AddMinutes(-1));

        var location = Assert.Single(department.DepartmentLocations);
        Assert.Equal(locationId, location.LocationId);
        Assert.Equal(department.Id, location.DepartmentId);

        var departmentsCount = await ExecuteInDbAsync(dbContext => dbContext.Departments.CountAsync(ct));
        Assert.Equal(1, departmentsCount);
    }

    [Fact]
    public async Task CreateDepartment_With_Multiple_Locations_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId1 = await CreateLocation(ct);
        var locationId2 = await CreateLocation(ct);

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Platform Engineering", "PLATENG", null, [locationId1, locationId2]));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(d => d.Id == result.Value, ct);
        });

        Assert.NotNull(department);
        Assert.Equal(2, department.DepartmentLocations.Count);
        Assert.Contains(department.DepartmentLocations, dl => dl.LocationId == locationId1);
        Assert.Contains(department.DepartmentLocations, dl => dl.LocationId == locationId2);
    }

    [Fact]
    public async Task CreateDepartment_With_Parent_Should_SetDepthAndPath()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocation(ct);

        var parentResult = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Engineering", "ENG", null, [locationId]));

            return sut.Handle(command, ct);
        });
        Assert.True(parentResult.IsSuccess);

        // Act
        var childResult = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Backend Team", "BACKEND", parentResult.Value, [locationId]));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(childResult.IsSuccess);

        var child = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Id == childResult.Value, ct);
        });

        Assert.NotNull(child);
        Assert.Equal(parentResult.Value, child.ParentId);
        Assert.Equal(1, child.Depth);
        Assert.Equal("ENG/BACKEND", child.Path.Value);
    }

    [Fact]
    public async Task CreateDepartment_With_NonExistingLocation_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingLocationId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Ghost Department", "GHOST", null, [nonExistingLocationId]));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);

        var departmentsCount = await ExecuteInDbAsync(dbContext => dbContext.Departments.CountAsync(ct));
        Assert.Equal(0, departmentsCount);
    }

    [Fact]
    public async Task CreateDepartment_With_NonExistingParent_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocation(ct);
        var nonExistingParentId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Orphan Department", "ORPHAN", nonExistingParentId, [locationId]));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);

        var departmentsCount = await ExecuteInDbAsync(dbContext => dbContext.Departments.CountAsync(ct));
        Assert.Equal(0, departmentsCount);
    }

    [Fact]
    public async Task CreateDepartment_With_DuplicateSlug_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocation(ct);

        var firstResult = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Enterprise Architecture", "ENTARCH", null, [locationId]));

            return sut.Handle(command, ct);
        });
        Assert.True(firstResult.IsSuccess);
        Assert.NotEqual(firstResult.Value, Guid.Empty);

        // Act
        // Slug matching is case-insensitive (ExistsBySlug uses ILIKE), so a differently-cased
        // slug for a different department name must still be rejected as a duplicate.
        var secondResult = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("Enterprise Architecture Duplicate", "entarch", null, [locationId]));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(secondResult.IsFailure);
        var error = Assert.Single(secondResult.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var departmentsCount = await ExecuteInDbAsync(dbContext => dbContext.Departments.CountAsync(ct));
        Assert.Equal(1, departmentsCount);
    }

    [Fact]
    public async Task CreateDepartment_With_No_Locations_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest("No Locations Department", "NOLOC", null, []));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var departmentsCount = await ExecuteInDbAsync(dbContext => dbContext.Departments.CountAsync(ct));
        Assert.Equal(0, departmentsCount);
    }

    private async Task<Guid> CreateLocation(CancellationToken ct)
    {
        var locationId = await ExecuteInDbAsync(async dbContext =>
        {
            var locationAddress = Address.Create(
                "00-124", "Poland", "Warsaw", "Rondo ONZ",
                "1", "Tower A", "1205", "PO Box 321");
            var locationTimezone = Timezone.Create("Europe/Warsaw");

            var location = Location.Create(
                    "Warsaw Technology Center", locationAddress.Value, locationTimezone.Value)
                .Value;

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync(ct);
            return location.Id;
        });
        return locationId;
    }
}