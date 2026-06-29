using FluentValidation;

namespace DirectoryService.Application.Locations.DeleteLocation;

public sealed class DeleteLocationValidator : AbstractValidator<DeleteLocationCommand>
{
    public DeleteLocationValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}