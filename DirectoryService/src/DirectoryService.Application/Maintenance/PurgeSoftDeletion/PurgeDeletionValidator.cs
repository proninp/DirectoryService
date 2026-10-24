using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Maintenance.PurgeSoftDeletion;

public sealed class PurgeDeletionValidator : AbstractValidator<PurgeDeletedRecordsCommand>
{
    public PurgeDeletionValidator(TimeProvider timeProvider)
    {
        RuleFor(c => c.BatchSize)
            .GreaterThan(0)
            .WithMessage(GeneralErrors.ValueIsInvalid(nameof(PurgeDeletedRecordsCommand.BatchSize))
                .ToString())
            .DependentRules(() =>
            {
                RuleFor(c => c.DeleteOlderThan)
                    .LessThan(_ => timeProvider.GetUtcNow().UtcDateTime)
                    .WithMessage(GeneralErrors.ValueIsInvalid(nameof(PurgeDeletedRecordsCommand.DeleteOlderThan))
                        .ToString());
            });
    }
}