using DirectoryService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DirectoryService.IntegrationTests.Infrastructure;

public abstract class DirectoryBaseTest : IClassFixture<TestWebFactory>, IAsyncLifetime
{
    protected IServiceProvider Services { get; private set; }

    private readonly Func<Task> _resetDatabase;

    protected DirectoryBaseTest(TestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    protected async Task<T> ExecuteHandlerAsync<T, THabdler>(Func<THabdler, Task<T>> func)
        where THabdler : notnull
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<THabdler>();
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