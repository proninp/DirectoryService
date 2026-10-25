using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace DirectoryService.IntegrationTests.SoftDeletion;

/// <summary>
/// Проверяет регистрацию из <see cref="Registration.AddSoftDeletionCleanup"/> на минимальном хосте.
/// WebApplicationFactory здесь не подходит: Program перехватывает исключение старта в catch,
/// и наружу доходит только ObjectDisposedException вместо OptionsValidationException.
/// </summary>
#pragma warning disable CA1707
public sealed class SoftDeletionOptionsRegistrationTest
{
    private const string Section = nameof(SoftDeletionCleanupRetention);

    [Fact]
    public void Validator_Should_Be_Registered()
    {
        // Arrange
        using var host = BuildHost([]);

        // Act
        var validators = host.Services.GetServices<IValidateOptions<SoftDeletionCleanupRetention>>();

        // Assert
        Assert.Contains(validators, v => v is SoftDeletionCleanupRetentionValidator);
    }

    [Fact]
    public void Options_Should_Be_Bound_From_Configuration()
    {
        // Arrange
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{Section}:Interval"] = "00:05:00",
            [$"{Section}:Timeout"] = "00:00:30",
            [$"{Section}:RetentionDays"] = "30",
            [$"{Section}:BatchSize"] = "100",
            [$"{Section}:Enabled"] = "false",
        });

        // Act
        var options = host.Services.GetRequiredService<IOptionsMonitor<SoftDeletionCleanupRetention>>().CurrentValue;

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(5), options.Interval);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.Equal(30, options.RetentionDays);
        Assert.Equal(100, options.BatchSize);
        Assert.False(options.Enabled);
    }

    [Fact]
    public async Task Host_With_Valid_Options_Should_Start()
    {
        // Arrange
        // Enabled = false: проверяется только старт, без обращения к хендлеру и БД
        using var host = BuildHost(new Dictionary<string, string?> { [$"{Section}:Enabled"] = "false" });

        // Act
        var exception = await Record.ExceptionAsync(() => host.StartAsync());
        await host.StopAsync();

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Interval", "00:00:00", "Interval must be positive")]
    [InlineData("Timeout", "01:00:00", "Timeout must be less than Interval")]
    [InlineData("RetentionDays", "0", "RetentionDays must be positive")]
    [InlineData("BatchSize", "0", "BatchSize must be positive")]
    public async Task Host_With_Invalid_Options_Should_Fail_On_Start(string key, string value, string expectedFailure)
    {
        // Arrange
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{Section}:Enabled"] = "false",
            [$"{Section}:{key}"] = value,
        });

        // Act
        var exception = await Record.ExceptionAsync(() => host.StartAsync());

        // Assert
        var validationException = Assert.IsType<OptionsValidationException>(exception);
        Assert.Equal(typeof(SoftDeletionCleanupRetention), validationException.OptionsType);
        Assert.Contains(validationException.Failures, f => f.Contains(expectedFailure, StringComparison.Ordinal));
    }

    private static IHost BuildHost(Dictionary<string, string?> settings)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(settings);

        builder.Services.AddLogging();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSoftDeletionCleanup(builder.Configuration);

        return builder.Build();
    }
}
