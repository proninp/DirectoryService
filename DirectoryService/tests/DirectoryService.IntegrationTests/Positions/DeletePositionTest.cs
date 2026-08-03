using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.DeletePosition;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Positions;

#pragma warning disable CA1707
public sealed class DeletePositionTest : DirectoryBaseTest
{
    public DeletePositionTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeletePosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync([], ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, DeletePositionCommand> sut) =>
        {
            var command = new DeletePositionCommand(positionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(positionId, result.Value);

        var position = await ExecuteInDbAsync(dbContext =>
            dbContext.Positions.FirstOrDefaultAsync(p => p.Id == positionId, ct));
        Assert.Null(position);
    }

    [Fact]
    public async Task DeletePosition_With_NonExistingPosition_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingPositionId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, DeletePositionCommand> sut) =>
        {
            var command = new DeletePositionCommand(nonExistingPositionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task DeletePosition_With_LinkedDepartments_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var positionId = await CreatePositionAsync([departmentId], ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, DeletePositionCommand> sut) =>
        {
            var command = new DeletePositionCommand(positionId);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var position = await ExecuteInDbAsync(dbContext =>
            dbContext.Positions.FirstOrDefaultAsync(p => p.Id == positionId, ct));
        Assert.NotNull(position);
    }

    [Fact]
    public async Task DeletePosition_With_EmptyId_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, DeletePositionCommand> sut) =>
        {
            var command = new DeletePositionCommand(Guid.Empty);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);
    }

    private async Task<Guid> CreatePositionAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken ct)
    {
        var positionId = await ExecuteInDbAsync(async dbContext =>
        {
            var newPositionId = Guid.NewGuid();
            var departmentPositions = departmentIds
                .Select(departmentId => new DepartmentPosition(departmentId, newPositionId))
                .ToList();

            var position = Position.Create(
                    newPositionId, "Software Engineer", "Writes code", departmentPositions)
                .Value;

            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync(ct);
            return position.Id;
        });

        return positionId;
    }

    private async Task<Guid> CreateDepartmentAsync(CancellationToken ct)
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
                    "Position Test Department", slug, [departmentLocation], departmentId: newDepartmentId)
                .Value;

            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(ct);
            return department.Id;
        });

        return departmentId;
    }
}
