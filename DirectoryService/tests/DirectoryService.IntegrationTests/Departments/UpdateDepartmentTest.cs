using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

#pragma warning disable CA1707
public sealed class UpdateDirectoryTest : DirectoryBaseTest
{
    public UpdateDirectoryTest(TestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartment_With_Valid_Data_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartment("Enterprise Architecture", "ENTARCH", ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateDepartmentHandler sut) =>
        {
            var command = new UpdateDepartmentCommand(
                departmentId, new UpdateDepartmentRequest("Enterprise Architecture Renamed"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId, result.Value.Id);
        Assert.Equal("Enterprise Architecture Renamed", result.Value.Name);

        var department = await AssertDepartmentNameAsync(departmentId, "Enterprise Architecture Renamed", ct);
        Assert.NotNull(department.UpdatedAt);
    }

    [Fact]
    public async Task UpdateDepartment_With_Same_Name_Should_Succeed()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartment("Enterprise Architecture", "ENTARCH", ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateDepartmentHandler sut) =>
        {
            var command = new UpdateDepartmentCommand(
                departmentId, new UpdateDepartmentRequest("Enterprise Architecture"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Enterprise Architecture", result.Value.Name);
    }

    [Fact]
    public async Task UpdateDepartment_With_NonExistingDepartment_Should_ReturnNotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var nonExistingDepartmentId = Guid.NewGuid();

        // Act
        var result = await ExecuteHandlerAsync((UpdateDepartmentHandler sut) =>
        {
            var command = new UpdateDepartmentCommand(
                nonExistingDepartmentId, new UpdateDepartmentRequest("New Name"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.NotFound, error.ErrorType);
    }

    [Fact]
    public async Task UpdateDepartment_With_Empty_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartment("Enterprise Architecture", "ENTARCH", ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateDepartmentHandler sut) =>
        {
            var command = new UpdateDepartmentCommand(departmentId, new UpdateDepartmentRequest(string.Empty));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        await AssertDepartmentNameAsync(departmentId, "Enterprise Architecture", ct);
    }

    [Fact]
    public async Task UpdateDepartment_With_TooShort_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartment("Enterprise Architecture", "ENTARCH", ct);

        // Act
        var result = await ExecuteHandlerAsync((UpdateDepartmentHandler sut) =>
        {
            var command = new UpdateDepartmentCommand(departmentId, new UpdateDepartmentRequest("AB"));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        await AssertDepartmentNameAsync(departmentId, "Enterprise Architecture", ct);
    }

    [Fact]
    public async Task UpdateDepartment_With_TooLong_Name_Should_ReturnValidationError()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartment("Enterprise Architecture", "ENTARCH", ct);
        var tooLongName = new string('A', Department.NameMaxLength + 1);

        // Act
        var result = await ExecuteHandlerAsync((UpdateDepartmentHandler sut) =>
        {
            var command = new UpdateDepartmentCommand(departmentId, new UpdateDepartmentRequest(tooLongName));

            return sut.Handle(command, ct);
        });

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Error);
        Assert.Equal(ErrorType.Validation, error.ErrorType);

        await AssertDepartmentNameAsync(departmentId, "Enterprise Architecture", ct);
    }

    private async Task<Guid> CreateDepartment(string name, string slug, CancellationToken ct)
    {
        var locationId = await CreateLocation(ct);

        var result = await ExecuteHandlerAsync((CreateDepartmentHandler sut) =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(name, slug, null, [locationId]));

            return sut.Handle(command, ct);
        });

        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private async Task<Department> AssertDepartmentNameAsync(Guid departmentId, string expectedName, CancellationToken ct)
    {
        var department = await ExecuteInDbAsync(dbContext =>
            dbContext.Departments.FirstOrDefaultAsync(d => d.Id == departmentId, ct));

        Assert.NotNull(department);
        Assert.Equal(expectedName, department.Name);
        return department;
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