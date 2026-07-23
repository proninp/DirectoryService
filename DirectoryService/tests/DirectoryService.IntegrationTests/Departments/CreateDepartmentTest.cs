using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

#pragma warning disable CA1707
public class CreateDepartmentTest : DepartmentBaseTest
{
    protected CreateDepartmentTest(TestWebFactory factory)
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
        var result = await ExecuteHandlerAsync(sut =>
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
                .FirstOrDefaultAsync(d => d.Id == result.Value, ct);
        });

        Assert.NotNull(department);
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