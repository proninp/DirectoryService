using DirectoryService.Application.Departments.DeleteDepartment;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

#pragma warning disable CA1707
public sealed class DeleteDirectoryTest : DirectoryBaseTest
{
    public DeleteDirectoryTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteDepartment_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentHandler sut) =>
        {
            var command = new DeleteDepartmentCommand(departmentId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId, result.Value);

        var department = await ExecuteInDbAsync(dbContext =>
            dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct));

        Assert.Null(department);
    }

    [Fact]
    public async Task DeleteDepartment_With_NonExistingDepartment_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingDepartmentId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentHandler sut) =>
        {
            var command = new DeleteDepartmentCommand(nonExistingDepartmentId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task DeleteDepartment_With_Locations_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct, withLocation: true);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentHandler sut) =>
        {
            var command = new DeleteDepartmentCommand(departmentId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var department = await ExecuteInDbAsync(dbContext =>
            dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct));

        Assert.NotNull(department);
    }

    [Fact]
    public async Task DeleteDepartment_With_Positions_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct, withPosition: true);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentHandler sut) =>
        {
            var command = new DeleteDepartmentCommand(departmentId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var department = await ExecuteInDbAsync(dbContext =>
            dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct));

        Assert.NotNull(department);
    }

    private async Task<Guid> CreateDepartmentAsync(
        CancellationToken ct,
        bool withLocation = false,
        bool withPosition = false)
    {
        var departmentId = await ExecuteInDbAsync(async dbContext =>
        {
            var locationAddress = Address.Create(
                "00-124", "Poland", "Warsaw", "Rondo ONZ",
                "1", "Tower A", "1205", "PO Box 321");
            var locationTimezone = Timezone.Create("Europe/Warsaw");

            var location = Location.Create(
                    "Warsaw Technology Center", locationAddress.Value, locationTimezone.Value)
                .Value;
            dbContext.Locations.Add(location);

            var newDepartmentId = Guid.NewGuid();
            var slug = Slug.Create("DELDEPT").Value;
            var departmentLocation = new DepartmentLocation(newDepartmentId, location.Id);

            var department = Department.Create(
                    "Department To Delete", slug, [departmentLocation], departmentId: newDepartmentId)
                .Value;

            if (withPosition)
            {
                var position = Position.Create(Guid.NewGuid(), "Position To Link", null).Value;
                dbContext.Positions.Add(position);
                department.AddPosition(position.Id);
            }

            if (!withLocation)
                department.RemoveLocation(location.Id);

            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(ct);
            return department.Id;
        });

        return departmentId;
    }
}