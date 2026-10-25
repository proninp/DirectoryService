using DirectoryService.Application.Maintenance.PurgeSoftDeletion;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace DirectoryService.IntegrationTests.SoftDeletion;

#pragma warning disable CA1707
public sealed class SoftDeletionBackgroundServiceTest : DirectoryBaseTest
{
    private const int RetentionDays = 60;

    private static readonly TimeSpan s_waitTimeout = TimeSpan.FromSeconds(15);

    // Интервал короткий, чтобы тесты с несколькими итерациями шли быстро;
    // Timeout < Interval, иначе опции не пройдут SoftDeletionCleanupRetentionValidator
    private static readonly SoftDeletionCleanupRetention s_defaultOptions = new()
    {
        Interval = TimeSpan.FromSeconds(1),
        Timeout = TimeSpan.FromMilliseconds(500),
        RetentionDays = RetentionDays,
        BatchSize = 500,
        Enabled = true,
    };

    private readonly TestLogger<SoftDeletionBackgroundService> _logger = new();

    private readonly IValidateOptions<SoftDeletionCleanupRetention> _optionsValidator;

    public SoftDeletionBackgroundServiceTest(TestWebFactory factory)
        : base(factory)
    {
        _optionsValidator = Services.GetRequiredService<IValidateOptions<SoftDeletionCleanupRetention>>();
    }

    [Fact]
    public async Task Purge_Should_Remove_Records_SoftDeleted_Before_Retention()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var locationId = await CreateLocationAsync(ct);
        var positionId = await CreatePositionAsync(ct);

        await MarkDeletedAsync<Department>(departmentId, DaysAgo(RetentionDays + 30), ct);
        await MarkDeletedAsync<Location>(locationId, DaysAgo(RetentionDays + 30), ct);
        await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays + 30), ct);

        // Act
        var summary = await RunUntilPurgeSummaryAsync(CreateOptions(s_defaultOptions));

        // Assert
        Assert.False(await ExistsAsync<Department>(departmentId, ct));
        Assert.False(await ExistsAsync<Location>(locationId, ct));
        Assert.False(await ExistsAsync<Position>(positionId, ct));

        Assert.Equal(LogLevel.Information, summary.Level);
        Assert.Equal(PurgeOutcome.Completed, summary.Properties["Outcome"]);
        Assert.Equal(3, summary.Properties["TotalPurged"]);
        Assert.Equal(1, summary.Properties["DepartmentsPurged"]);
        Assert.Equal(1, summary.Properties["LocationsPurged"]);
        Assert.Equal(1, summary.Properties["PositionsPurged"]);
        Assert.Equal(3, summary.Properties["EntitySetsProcessed"]);
        Assert.Equal(0, summary.Properties["EntitySetsSkipped"]);
        Assert.Equal(0, summary.Properties["EntitySetsFailed"]);
    }

    [Fact]
    public async Task Purge_Should_Keep_Records_SoftDeleted_Within_Retention()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var locationId = await CreateLocationAsync(ct);
        var positionId = await CreatePositionAsync(ct);

        await MarkDeletedAsync<Department>(departmentId, DaysAgo(RetentionDays - 10), ct);
        await MarkDeletedAsync<Location>(locationId, DaysAgo(RetentionDays - 10), ct);
        await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays - 10), ct);

        // Act
        var summary = await RunUntilPurgeSummaryAsync(CreateOptions(s_defaultOptions));

        // Assert
        Assert.True(await ExistsAsync<Department>(departmentId, ct));
        Assert.True(await ExistsAsync<Location>(locationId, ct));
        Assert.True(await ExistsAsync<Position>(positionId, ct));

        Assert.Equal(PurgeOutcome.Completed, summary.Properties["Outcome"]);
        Assert.Equal(0, summary.Properties["TotalPurged"]);
    }

    [Fact]
    public async Task Purge_Should_Keep_Active_Records()
    {
        // Arrange
        var ct = CancellationToken.None;
        var departmentId = await CreateDepartmentAsync(ct);
        var locationId = await CreateLocationAsync(ct);
        var positionId = await CreatePositionAsync(ct);

        // Act
        var summary = await RunUntilPurgeSummaryAsync(CreateOptions(s_defaultOptions));

        // Assert
        Assert.True(await ExistsAsync<Department>(departmentId, ct));
        Assert.True(await ExistsAsync<Location>(locationId, ct));
        Assert.True(await ExistsAsync<Position>(positionId, ct));

        Assert.Equal(0, summary.Properties["TotalPurged"]);
    }

    [Fact]
    public async Task Purge_Should_Remove_All_Records_When_They_Exceed_BatchSize()
    {
        // Arrange
        var ct = CancellationToken.None;
        const int positionsCount = 5;
        var positionIds = new List<Guid>();
        for (var i = 0; i < positionsCount; i++)
        {
            var positionId = await CreatePositionAsync(ct);
            await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays + 1), ct);
            positionIds.Add(positionId);
        }

        var options = s_defaultOptions with { BatchSize = 2 };

        // Act
        var summary = await RunUntilPurgeSummaryAsync(CreateOptions(options));

        // Assert
        foreach (var positionId in positionIds)
            Assert.False(await ExistsAsync<Position>(positionId, ct));

        Assert.Equal(positionsCount, summary.Properties["PositionsPurged"]);
        Assert.Equal(positionsCount, summary.Properties["TotalPurged"]);
    }

    [Fact]
    public async Task Purge_Should_Calculate_Threshold_From_TimeProvider()
    {
        // Arrange
        var ct = CancellationToken.None;
        var shiftedNow = DateTimeOffset.UtcNow.AddDays(30);

        // Относительно реального времени запись в пределах retention, относительно TimeProvider — уже нет
        var positionId = await CreatePositionAsync(ct);
        await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays - 10), ct);

        // Act
        var summary = await RunUntilPurgeSummaryAsync(
            CreateOptions(s_defaultOptions),
            new FixedTimeProvider(shiftedNow));

        // Assert
        Assert.False(await ExistsAsync<Position>(positionId, ct));
        Assert.Equal(1, summary.Properties["PositionsPurged"]);
    }

    [Fact]
    public async Task Purge_Should_Not_Run_When_Disabled()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays + 30), ct);

        var options = CreateOptions(
            s_defaultOptions with { Enabled = false });

        // Act
        // Первое чтение — для таймера, каждое следующее — отдельная итерация
        await RunUntilAsync(options, () => options.ReadCount >= 4, "three disabled iterations");

        // Assert
        Assert.True(await ExistsAsync<Position>(positionId, ct));
        Assert.DoesNotContain(_logger.Entries, IsPurgeSummary);
    }

    [Fact]
    public async Task Purge_Should_Start_When_Enabled_At_Runtime()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays + 30), ct);

        var options = CreateOptions(readNumber =>
            s_defaultOptions with { Enabled = readNumber > 3 });

        // Act
        var summary = await RunUntilPurgeSummaryAsync(options);

        // Assert
        Assert.True(options.ReadCount > 3);
        Assert.False(await ExistsAsync<Position>(positionId, ct));
        Assert.Equal(1, summary.Properties["PositionsPurged"]);
    }

    [Fact]
    public async Task Purge_Should_Log_Rejection_When_Command_Is_Invalid()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays + 30), ct);

        // Валидатор опций такой BatchSize не пропустит, поэтому монитор создаётся без него:
        // тест проверяет второй рубеж — валидацию самой команды
        var options = new TestOptionsMonitor<SoftDeletionCleanupRetention>(s_defaultOptions with { BatchSize = 0 });

        // Act
        var rejection = await RunUntilLogAsync(
            options,
            e => e.Level == LogLevel.Warning && e.Properties.ContainsKey("@Errors"),
            "command rejection warning");

        // Assert
        Assert.NotNull(rejection.Properties["@Errors"]);
        Assert.True(await ExistsAsync<Position>(positionId, ct));
        Assert.DoesNotContain(_logger.Entries, IsPurgeSummary);
    }

    [Fact]
    public async Task Purge_Should_Continue_After_Invalid_Options_Reload()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = await CreatePositionAsync(ct);
        await MarkDeletedAsync<Position>(positionId, DaysAgo(RetentionDays + 30), ct);

        // Чтения 2 и 3 (первые две итерации) эмулируют невалидный конфиг после hot reload
        var options = CreateOptions(readNumber =>
            readNumber is 2 or 3
                ? s_defaultOptions with { BatchSize = 0 }
                : s_defaultOptions);

        // Act
        var summary = await RunUntilPurgeSummaryAsync(options);

        // Assert
        var optionErrors = _logger.Entries
            .Where(e => e.Level == LogLevel.Error && e.Properties.ContainsKey("@Failures"))
            .ToList();
        Assert.Equal(2, optionErrors.Count);
        Assert.All(optionErrors, e =>
            Assert.Contains(
                Assert.IsAssignableFrom<IEnumerable<string>>(e.Properties["@Failures"]),
                f => f.Contains("BatchSize must be positive", StringComparison.Ordinal)));

        Assert.False(await ExistsAsync<Position>(positionId, ct));
        Assert.Equal(1, summary.Properties["PositionsPurged"]);
    }

    private static bool IsPurgeSummary(TestLogEntry entry) => entry.Properties.ContainsKey("TotalPurged");

    // Опции проходят через тот же валидатор, что зарегистрирован в DI приложения
    private TestOptionsMonitor<SoftDeletionCleanupRetention> CreateOptions(SoftDeletionCleanupRetention options) =>
        new(options, _optionsValidator);

    private TestOptionsMonitor<SoftDeletionCleanupRetention> CreateOptions(
        Func<int, SoftDeletionCleanupRetention> valueFactory) =>
        new(valueFactory, _optionsValidator);

    private static DateTime DaysAgo(int days) => DateTime.UtcNow.AddDays(-days);

    private Task<TestLogEntry> RunUntilPurgeSummaryAsync(
        IOptionsMonitor<SoftDeletionCleanupRetention> options, TimeProvider? timeProvider = null) =>
        RunUntilLogAsync(options, IsPurgeSummary, "purge summary log", timeProvider);

    private async Task<TestLogEntry> RunUntilLogAsync(
        IOptionsMonitor<SoftDeletionCleanupRetention> options,
        Func<TestLogEntry, bool> predicate,
        string description,
        TimeProvider? timeProvider = null)
    {
        await RunUntilAsync(options, () => _logger.Entries.Any(predicate), description, timeProvider);
        return _logger.Entries.First(predicate);
    }

    private async Task RunUntilAsync(
        IOptionsMonitor<SoftDeletionCleanupRetention> options,
        Func<bool> condition,
        string description,
        TimeProvider? timeProvider = null)
    {
        using var service = new SoftDeletionBackgroundService(
            options,
            Services.GetRequiredService<IServiceScopeFactory>(),
            timeProvider ?? TimeProvider.System,
            _logger);

        await service.StartAsync(CancellationToken.None);
        try
        {
            await TestWait.UntilAsync(condition, s_waitTimeout, description);
        }
        catch (TimeoutException e)
        {
            // Без логов сервиса по таймауту не понять, что пошло не так внутри итерации
            throw new TimeoutException($"{e.Message}{Environment.NewLine}Captured logs:{Environment.NewLine}{_logger.Dump()}", e);
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }
    }

    private Task<bool> ExistsAsync<TEntity>(Guid id, CancellationToken ct)
        where TEntity : BaseEntity =>
        ExecuteInDbAsync(dbContext => dbContext.Set<TEntity>()
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Id == id, ct));

    private Task<int> MarkDeletedAsync<TEntity>(Guid id, DateTime deletedAt, CancellationToken ct)
        where TEntity : BaseEntity =>
        ExecuteInDbAsync(dbContext => dbContext.Set<TEntity>()
            .IgnoreQueryFilters()
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(e => e.IsActive, false)
                    .SetProperty(e => e.DeletedAt, deletedAt),
                ct));

    private Task<Guid> CreateLocationAsync(CancellationToken ct) =>
        ExecuteInDbAsync(async dbContext =>
        {
            var location = BuildLocation();
            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync(ct);
            return location.Id;
        });

    private Task<Guid> CreatePositionAsync(CancellationToken ct) =>
        ExecuteInDbAsync(async dbContext =>
        {
            var position = Position.Create(Guid.NewGuid(), $"Position {Guid.NewGuid()}", null).Value;
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync(ct);
            return position.Id;
        });

    // Отдел создаётся со своей активной локацией: без локации домен его не создаст
    private Task<Guid> CreateDepartmentAsync(CancellationToken ct) =>
        ExecuteInDbAsync(async dbContext =>
        {
            var location = BuildLocation();
            dbContext.Locations.Add(location);

            var departmentId = Guid.NewGuid();
            var slugSuffix = new string([.. Guid.NewGuid().ToByteArray().Select(b => (char)('A' + (b % 26)))]);
            var slug = Slug.Create($"DEPT{slugSuffix}").Value;
            var department = Department.Create(
                    "Soft Deletion Department", slug, [new DepartmentLocation(departmentId, location.Id)],
                    departmentId: departmentId)
                .Value;
            dbContext.Departments.Add(department);

            await dbContext.SaveChangesAsync(ct);
            return department.Id;
        });

    private static Location BuildLocation()
    {
        var address = Address.Create(
                "00-124", "Poland", "Warsaw", "Rondo ONZ",
                Guid.NewGuid().ToString("N")[..8], "Tower A", "1205", "PO Box 321")
            .Value;
        var timezone = Timezone.Create("Europe/Warsaw").Value;

        return Location.Create($"Location {Guid.NewGuid()}", address, timezone).Value;
    }
}
