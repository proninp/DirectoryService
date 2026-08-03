using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Positions;

#pragma warning disable CA1707
public sealed class CreatePositionTest : DirectoryBaseTest
{
    public CreatePositionTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreatePosition_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var request = new CreatePositionRequest("Software Engineer", "Writes code", [departmentId]);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var position = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Positions
                .Include(p => p.DepartmentPositions)
                .FirstOrDefaultAsync(p => p.Id == result.Value, ct);
        });

        Assert.NotNull(position);
        Assert.Equal("Software Engineer", position.Name);
        Assert.Equal("Writes code", position.Description);

        Assert.True(position.IsActive);
        Assert.Null(position.DeletedAt);
        Assert.Null(position.UpdatedAt);
        Assert.True(position.CreatedAt > DateTime.UtcNow.AddMinutes(-1));

        var departmentPosition = Assert.Single(position.DepartmentPositions);
        Assert.Equal(departmentId, departmentPosition.DepartmentId);
        Assert.Equal(position.Id, departmentPosition.PositionId);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(1, positionsCount);
    }

    [Fact]
    public async Task CreatePosition_With_Multiple_Departments_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId1 = await CreateDepartmentAsync(ct);
        var departmentId2 = await CreateDepartmentAsync(ct);
        var request = new CreatePositionRequest(
            "Engineering Manager", "Manages teams", [departmentId1, departmentId2]);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);

        var position = await ExecuteInDbAsync(async dbContext =>
        {
            return await dbContext.Positions
                .Include(p => p.DepartmentPositions)
                .FirstOrDefaultAsync(p => p.Id == result.Value, ct);
        });

        Assert.NotNull(position);
        Assert.Equal(2, position.DepartmentPositions.Count);
        Assert.Contains(position.DepartmentPositions, dp => dp.DepartmentId == departmentId1);
        Assert.Contains(position.DepartmentPositions, dp => dp.DepartmentId == departmentId2);
    }

    [Fact]
    public async Task CreatePosition_With_NonExistingDepartment_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingDepartmentId = Guid.NewGuid();
        var request = new CreatePositionRequest("Ghost Position", "Description", [nonExistingDepartmentId]);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(0, positionsCount);
    }

    [Fact]
    public async Task CreatePosition_With_DuplicateName_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);

        var firstResult = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(
                new CreatePositionRequest("Software Engineer", "Writes code", [departmentId]));

            return sut.Handle(command, ct);
        });
        Assert.True(firstResult.IsSuccess);

        // Act
        var secondResult = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(
                new CreatePositionRequest("Software Engineer", "Another description", [departmentId]));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(secondResult.IsFailure);
        var error = Assert.Single(secondResult.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(1, positionsCount);
    }

    [Fact]
    public async Task CreatePosition_With_No_Departments_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var request = new CreatePositionRequest("No Departments Position", "Description", []);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(0, positionsCount);
    }

    [Fact]
    public async Task CreatePosition_With_DuplicateDepartmentIds_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var request = new CreatePositionRequest(
            "Duplicate Departments Position", "Description", [departmentId, departmentId]);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(0, positionsCount);
    }

    [Fact]
    public async Task CreatePosition_With_TooShort_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var request = new CreatePositionRequest("AB", "Description", [departmentId]);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(0, positionsCount);
    }

    [Fact]
    public async Task CreatePosition_With_TooLong_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var tooLongName = new string('A', Position.NameMaxLength + 1);
        var request = new CreatePositionRequest(tooLongName, "Description", [departmentId]);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(0, positionsCount);
    }

    [Fact]
    public async Task CreatePosition_With_TooLong_Description_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var tooLongDescription = new string('A', Position.DescriptionMaxLength + 1);
        var request = new CreatePositionRequest("Valid Name", tooLongDescription, [departmentId]);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<Guid, CreatePositionCommand> sut) =>
        {
            var command = new CreatePositionCommand(request);

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var positionsCount = await ExecuteInDbAsync(dbContext => dbContext.Positions.CountAsync(ct));
        Assert.Equal(0, positionsCount);
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