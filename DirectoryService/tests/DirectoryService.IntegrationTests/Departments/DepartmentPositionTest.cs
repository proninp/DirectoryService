using DirectoryService.Application.Departments.CreateDepartmentPosition;
using DirectoryService.Application.Departments.DeleteDepartmentPosition;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

#pragma warning disable CA1707
public sealed class DepartmentPositionTest : DirectoryBaseTest
{
    public DepartmentPositionTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartmentPosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync([], ct);
        var positionId = await CreatePositionAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentPositionHandler sut) =>
        {
            var command = new CreateDepartmentPositionCommand(departmentId, positionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId, result.Value.Id);
        var positionIdInResponse = Assert.Single(result.Value.PositionIds);
        Assert.Equal(positionId, positionIdInResponse);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentPositions)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        var departmentPosition = Assert.Single(department.DepartmentPositions);
        Assert.Equal(positionId, departmentPosition.PositionId);
        Assert.Equal(departmentId, departmentPosition.DepartmentId);
    }

    [Fact]
    public async Task CreateDepartmentPosition_With_Multiple_Positions_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId1 = await CreatePositionAsync(ct);
        var departmentId = await CreateDepartmentAsync([positionId1], ct);
        var positionId2 = await CreatePositionAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentPositionHandler sut) =>
        {
            var command = new CreateDepartmentPositionCommand(departmentId, positionId2);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.PositionIds.Count);
        Assert.Contains(positionId1, result.Value.PositionIds);
        Assert.Contains(positionId2, result.Value.PositionIds);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentPositions)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        Assert.Equal(2, department.DepartmentPositions.Count);
        Assert.Contains(department.DepartmentPositions, dp => dp.PositionId == positionId2);
    }

    [Fact]
    public async Task CreateDepartmentPosition_With_NonExistingDepartment_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        var nonExistingDepartmentId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentPositionHandler sut) =>
        {
            var command = new CreateDepartmentPositionCommand(nonExistingDepartmentId, positionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task CreateDepartmentPosition_With_NonExistingPosition_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync([], ct);
        var nonExistingPositionId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentPositionHandler sut) =>
        {
            var command = new CreateDepartmentPositionCommand(departmentId, nonExistingPositionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentPositions)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        Assert.Empty(department.DepartmentPositions);
    }

    [Fact]
    public async Task CreateDepartmentPosition_With_ExistingRelation_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        var departmentId = await CreateDepartmentAsync([positionId], ct);

        // Act
        var result = await ExecuteHandlerAsync((CreateDepartmentPositionHandler sut) =>
        {
            var command = new CreateDepartmentPositionCommand(departmentId, positionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);
    }

    [Fact]
    public async Task DeleteDepartmentPosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId1 = await CreatePositionAsync(ct);
        var positionId2 = await CreatePositionAsync(ct);
        var departmentId = await CreateDepartmentAsync([positionId1, positionId2], ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentPositionHandler sut) =>
        {
            var command = new DeleteDepartmentPositionCommand(departmentId, positionId2);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId, result.Value.Id);
        var remainingPositionId = Assert.Single(result.Value.PositionIds);
        Assert.Equal(positionId1, remainingPositionId);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentPositions)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        var departmentPosition = Assert.Single(department.DepartmentPositions);
        Assert.Equal(positionId1, departmentPosition.PositionId);
    }

    [Fact]
    public async Task DeleteDepartmentPosition_With_NonExistingDepartment_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        var nonExistingDepartmentId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentPositionHandler sut) =>
        {
            var command = new DeleteDepartmentPositionCommand(nonExistingDepartmentId, positionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task DeleteDepartmentPosition_With_NonExistingRelation_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync([], ct);
        var otherPositionId = await CreatePositionAsync(ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentPositionHandler sut) =>
        {
            var command = new DeleteDepartmentPositionCommand(departmentId, otherPositionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task DeleteDepartmentPosition_With_OnlyPosition_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        var departmentId = await CreateDepartmentAsync([positionId], ct);

        // Act
        var result = await ExecuteHandlerAsync((DeleteDepartmentPositionHandler sut) =>
        {
            var command = new DeleteDepartmentPositionCommand(departmentId, positionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.PositionIds);

        var department = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Departments
                .Include(d => d.DepartmentPositions)
                .FirstOrDefaultAsync(d => d.Id == departmentId, ct);
        });

        Assert.NotNull(department);
        Assert.Empty(department.DepartmentPositions);
    }

    private async Task<Guid> CreatePositionAsync(CancellationToken ct)
    {
        var positionId = await ExecuteInDbAsync(async dbContext =>
        {
            var position = Position.Create(Guid.NewGuid(), $"Position {Guid.NewGuid()}", null).Value;

            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync(ct);
            return position.Id;
        });
        return positionId;
    }

    private async Task<Guid> CreateDepartmentAsync(IReadOnlyCollection<Guid> positionIds, CancellationToken ct)
    {
        var departmentId = await ExecuteInDbAsync(async dbContext =>
        {
            var locationAddress = Address.Create(
                "00-124", "Poland", "Warsaw", "Rondo ONZ",
                Guid.NewGuid().ToString("N")[..8], "Tower A", "1205", "PO Box 321");
            var locationTimezone = Timezone.Create("Europe/Warsaw");

            var location = Location.Create(
                    $"Warsaw Technology Center {Guid.NewGuid()}", locationAddress.Value, locationTimezone.Value)
                .Value;
            dbContext.Locations.Add(location);

            var newDepartmentId = Guid.NewGuid();
            var slugSuffix = new string([.. Guid.NewGuid().ToByteArray().Select(b => (char)('A' + (b % 26)))]);
            var slug = Slug.Create($"DEPT{slugSuffix}").Value;
            var departmentLocation = new DepartmentLocation(newDepartmentId, location.Id);

            var department = Department.Create(
                    "Department With Positions", slug, [departmentLocation], departmentId: newDepartmentId)
                .Value;

            foreach (var positionId in positionIds)
                department.AddPosition(positionId);

            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(ct);
            return department.Id;
        });

        return departmentId;
    }
}
