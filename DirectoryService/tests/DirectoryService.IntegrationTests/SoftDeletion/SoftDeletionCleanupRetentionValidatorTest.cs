using DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;
using Xunit;

namespace DirectoryService.IntegrationTests.SoftDeletion;

#pragma warning disable CA1707
public sealed class SoftDeletionCleanupRetentionValidatorTest
{
    private static readonly SoftDeletionCleanupRetention s_validOptions = new()
    {
        Interval = TimeSpan.FromMinutes(10),
        Timeout = TimeSpan.FromSeconds(20),
        RetentionDays = 60,
        BatchSize = 500,
        Enabled = true,
    };

    private readonly SoftDeletionCleanupRetentionValidator _sut = new();

    public static TheoryData<SoftDeletionCleanupRetention, string> InvalidOptions => new()
    {
        { s_validOptions with { Interval = TimeSpan.Zero }, "Interval must be positive" },
        { s_validOptions with { Interval = TimeSpan.FromSeconds(-1) }, "Interval must be positive" },
        { s_validOptions with { Timeout = TimeSpan.Zero }, "Timeout must be positive" },
        { s_validOptions with { Timeout = s_validOptions.Interval }, "Timeout must be less than Interval" },
        { s_validOptions with { Timeout = s_validOptions.Interval * 2 }, "Timeout must be less than Interval" },
        { s_validOptions with { RetentionDays = 0 }, "RetentionDays must be positive" },
        { s_validOptions with { RetentionDays = -1 }, "RetentionDays must be positive" },
        { s_validOptions with { BatchSize = 0 }, "BatchSize must be positive" },
        { s_validOptions with { BatchSize = -1 }, "BatchSize must be positive" },
    };

    [Fact]
    public void Validate_With_Valid_Options_Should_Succeed()
    {
        // Act
        var result = _sut.Validate(null, s_validOptions);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_With_Default_Options_Should_Succeed()
    {
        // Значения по умолчанию используются, если секции нет в конфиге
        var result = _sut.Validate(null, new SoftDeletionCleanupRetention());

        Assert.True(result.Succeeded);
    }

    [Theory]
    [MemberData(nameof(InvalidOptions))]
    public void Validate_With_Invalid_Options_Should_Fail(SoftDeletionCleanupRetention options, string expectedFailure)
    {
        // Act
        var result = _sut.Validate(null, options);

        // Assert
        // Не Single: при невалидном Interval дополнительно срабатывает правило Timeout < Interval
        Assert.True(result.Failed);
        Assert.Contains(result.Failures!, f => f.Contains(expectedFailure, StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_With_Several_Invalid_Values_Should_Report_All_Failures()
    {
        // Arrange
        var options = s_validOptions with { Interval = TimeSpan.Zero, RetentionDays = 0, BatchSize = 0 };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains(result.Failures!, f => f.Contains("Interval must be positive", StringComparison.Ordinal));
        Assert.Contains(result.Failures!, f => f.Contains("RetentionDays must be positive", StringComparison.Ordinal));
        Assert.Contains(result.Failures!, f => f.Contains("BatchSize must be positive", StringComparison.Ordinal));
    }
}
