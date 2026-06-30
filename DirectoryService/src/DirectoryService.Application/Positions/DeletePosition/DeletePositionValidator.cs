using FluentValidation;

namespace DirectoryService.Application.Positions.DeletePosition;

public sealed class DeletePositionValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}