using DirectoryService.Application.Locations.DeleteLocation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Locations;

#pragma warning disable CA1707
public sealed class DeleteLocationsTest : DirectoryBaseTest
{
    public DeleteLocationsTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteLocation_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteLocationHandler sut) =>
        {
            var command = new DeleteLocationCommand(locationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(locationId, result.Value);

        var location = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations.FirstOrDefaultAsync(l => l.Id == locationId, ct));

        Assert.Null(location);

        var deletedLocation = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == locationId, ct));

        Assert.NotNull(deletedLocation);
        Assert.False(deletedLocation.IsActive);
        Assert.NotNull(deletedLocation.DeletedAt);
    }

    [Fact]
    public async Task DeleteLocation_With_NonExistingLocation_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingLocationId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((DeleteLocationHandler sut) =>
        {
            var command = new DeleteLocationCommand(nonExistingLocationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task DeleteLocation_With_Departments_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var locationId = await CreateLocationAsync(ct, withDepartment: true);

        // Act
        var result = await ExecuteHandlerAsync((DeleteLocationHandler sut) =>
        {
            var command = new DeleteLocationCommand(locationId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var location = await ExecuteInDbAsync(dbContext =>
            dbContext.Locations.FirstOrDefaultAsync(l => l.Id == locationId, ct));

        Assert.NotNull(location);
        Assert.True(location.IsActive);
        Assert.Null(location.DeletedAt);
    }

    private async Task<Guid> CreateLocationAsync(CancellationToken ct, bool withDepartment = false)
    {
        var locationId = await ExecuteInDbAsync(async dbContext =>
        {
            var address = Address.Create(
                    "00-124", "Poland", "Warsaw", "Rondo ONZ",
                    "1", "Tower A", "1205", "PO Box 321")
                .Value;
            var timezone = Timezone.Create("Europe/Warsaw").Value;

            var location = Location.Create("Location To Delete", address, timezone).Value;
            dbContext.Locations.Add(location);

            if (withDepartment)
            {
                var departmentId = Guid.NewGuid();
                var slug = Slug.Create("DELLOC").Value;
                var departmentLocation = new DepartmentLocation(departmentId, location.Id);

                var department = Department.Create(
                        "Department Linked To Location", slug, [departmentLocation], departmentId: departmentId)
                    .Value;

                dbContext.Departments.Add(department);
            }

            await dbContext.SaveChangesAsync(ct);
            return location.Id;
        });

        return locationId;
    }
}