using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DirectoryService.IntegrationTests.Departments;

public class DepartmentBaseTest : IClassFixture<TestWebFactory>, IAsyncLifetime
{
    protected IServiceProvider Services { get; private set; }

    private readonly Func<Task> _resetDatabase;

    protected DepartmentBaseTest(TestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    protected async Task<T> ExecuteHandlerAsync<T>(Func<CreateDepartmentHandler, Task<T>> func)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        return await func(sut);
    }

    protected async Task<T> ExecuteInDbAsync<T>(Func<DirectoryServiceDbContext, Task<T>> func)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();
        return await func(sut);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabase();
}