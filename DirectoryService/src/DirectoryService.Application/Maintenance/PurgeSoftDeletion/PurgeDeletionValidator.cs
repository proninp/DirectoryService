using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Maintenance.PurgeSoftDeletion;

public sealed class PurgeDeletionValidator : AbstractValidator<PurgeDeletedRecordsCommand>
{
    public PurgeDeletionValidator()
    {
        RuleFor(c => c.BatchSize)
            .GreaterThan(0)
            .WithMessage(GeneralErrors.ValueIsInvalid(nameof(PurgeDeletedRecordsCommand.BatchSize))
                .ToString())
            .DependentRules(() =>
            {
                RuleFor(c => c.DeleteOlderThen)
                    .NotNull()
                    .LessThan(DateTime.UtcNow)
                    .WithMessage(GeneralErrors.ValueIsInvalid(nameof(PurgeDeletedRecordsCommand.DeleteOlderThen))
                        .ToString());
            });
    }
}