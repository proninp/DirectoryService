using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices.SoftDeletion;

public sealed class SoftDeletionCleanupRetentionValidator : IValidateOptions<SoftDeletionCleanupRetention>
{
    public ValidateOptionsResult Validate(string? name, SoftDeletionCleanupRetention options)
    {
        var builder = new ValidateOptionsResultBuilder();
        
        if (options.Interval <= TimeSpan.Zero)
            builder.AddError("Interval must be positive", nameof(options.Interval));

        if (options.Timeout <= TimeSpan.Zero)
            builder.AddError("Timeout must be positive", nameof(options.Timeout));
        else if (options.Timeout >= options.Interval)
            builder.AddError("Timeout must be less than Interval", nameof(options.Timeout));
        
        if (options.RetentionDays <= 0)
            builder.AddError("RetentionDays must be positive", nameof(options.RetentionDays));
        
        if (options.BatchSize <= 0)
            builder.AddError("BatchSize must be positive", nameof(options.BatchSize));
        
        return builder.Build();
    }
}