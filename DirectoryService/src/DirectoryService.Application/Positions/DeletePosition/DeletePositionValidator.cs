using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions.DeletePosition;

public sealed class DeletePositionValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired(nameof(DeletePositionCommand.Id)));
    }
}