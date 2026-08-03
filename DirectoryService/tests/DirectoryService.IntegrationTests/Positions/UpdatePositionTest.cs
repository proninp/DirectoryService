using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.UpdatePosition;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Contracts.Positions.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Positions;

#pragma warning disable CA1707
public sealed class UpdatePositionTest : DirectoryBaseTest
{
    public UpdatePositionTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePosition_With_Valid_Name_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(
                positionId, new UpdatePositionRequest("Senior Software Engineer", null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(positionId, result.Value.Id);
        Assert.Equal("Senior Software Engineer", result.Value.Name);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Senior Software Engineer", position.Name);
        Assert.Equal("Writes code", position.Description);
        Assert.NotNull(position.UpdatedAt);
    }

    [Fact]
    public async Task UpdatePosition_With_Valid_Description_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(
                positionId, new UpdatePositionRequest(null, "Writes tested code"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Software Engineer", result.Value.Name);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Software Engineer", position.Name);
        Assert.Equal("Writes tested code", position.Description);
        Assert.NotNull(position.UpdatedAt);
    }

    [Fact]
    public async Task UpdatePosition_With_Valid_Name_And_Description_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(
                positionId, new UpdatePositionRequest("Senior Software Engineer", "Writes tested code"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Senior Software Engineer", position.Name);
        Assert.Equal("Writes tested code", position.Description);
    }

    [Fact]
    public async Task UpdatePosition_With_Same_Name_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(
                positionId, new UpdatePositionRequest("Software Engineer", null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Software Engineer", result.Value.Name);
    }

    [Fact]
    public async Task UpdatePosition_With_NonExistingPosition_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingPositionId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(
                nonExistingPositionId, new UpdatePositionRequest("New Name", null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task UpdatePosition_With_DuplicateName_Should_ReturnConflict()
    {
        // Arrange
        var ct = CancellationToken.None;
        await CreatePositionAsync("Software Engineer", "Writes code", ct);
        var positionId = await CreatePositionAsync("QA Engineer", "Tests code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(
                positionId, new UpdatePositionRequest("Software Engineer", null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Conflict, error.ErrorType);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("QA Engineer", position.Name);
    }

    [Fact]
    public async Task UpdatePosition_With_Null_Name_And_Null_Description_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(positionId, new UpdatePositionRequest(null, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);
    }

    [Fact]
    public async Task UpdatePosition_With_Empty_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(positionId, new UpdatePositionRequest(string.Empty, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Software Engineer", position.Name);
    }

    [Fact]
    public async Task UpdatePosition_With_TooShort_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(positionId, new UpdatePositionRequest("AB", null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Software Engineer", position.Name);
    }

    [Fact]
    public async Task UpdatePosition_With_TooLong_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);
        var tooLongName = new string('A', Position.NameMaxLength + 1);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(positionId, new UpdatePositionRequest(tooLongName, null));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Software Engineer", position.Name);
    }

    [Fact]
    public async Task UpdatePosition_With_TooLong_Description_And_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);
        var tooLongDescription = new string('A', Position.DescriptionMaxLength + 1);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(
                positionId, new UpdatePositionRequest("New Name", tooLongDescription));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Software Engineer", position.Name);
        Assert.Equal("Writes code", position.Description);
    }

    [Fact]
    public async Task UpdatePosition_With_TooLong_Description_Only_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync("Software Engineer", "Writes code", ct);
        var tooLongDescription = new string('A', Position.DescriptionMaxLength + 1);

        // Act
        var result = await ExecuteHandlerAsync((ICommandHandler<PositionResponse, UpdatePositionCommand> sut) =>
        {
            var command = new UpdatePositionCommand(positionId, new UpdatePositionRequest(null, tooLongDescription));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        var position = await AssertPositionAsync(positionId, ct);
        Assert.Equal("Writes code", position.Description);
    }

    private async Task<Guid> CreatePositionAsync(string name, string? description, CancellationToken ct)
    {
        var departmentId = await CreateDepartmentAsync(ct);

        var positionId = await ExecuteInDbAsync(async dbContext =>
        {
            var newPositionId = Guid.NewGuid();
            var departmentPosition = new DepartmentPosition(departmentId, newPositionId);

            var position = Position.Create(newPositionId, name, description, [departmentPosition]).Value;

            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync(ct);
            return position.Id;
        });

        return positionId;
    }

    private async Task<Position> AssertPositionAsync(Guid positionId, CancellationToken ct)
    {
        var position = await ExecuteInDbAsync(dbContext =>
            dbContext.Positions.FirstOrDefaultAsync(p => p.Id == positionId, ct));

        Assert.NotNull(position);
        return position;
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
