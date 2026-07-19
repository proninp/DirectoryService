using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DirectoryService.IntegrationTests;

public class CreateDirectoryTest : DirectoryTestWebFactory
{
    [Fact]
    public async Task Test1()
    {
        // Arrange

        await using (var initializationScope = Services.CreateAsyncScope())
        {
            var dbContext = initializationScope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        }

        await using var scope = Services.CreateAsyncScope();

        var ct = CancellationToken.None;

        var createDepartmentHandler = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        var command = new CreateDepartmentCommand(new CreateDepartmentRequest("External Audit", "EAUDIT", null, []));

        // Act
        var result = await createDepartmentHandler.Handle(command, ct);

        // Assert
        Assert.Empty(result.Error);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);
    }
}