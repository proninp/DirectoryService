using FluentValidation;

namespace DirectoryService.Application.Departments.DeleteDepartmentPosition;

public sealed class DeleteDepartmentPositionValidator : AbstractValidator<DeleteDepartmentPositionCommand>
{
    public DeleteDepartmentPositionValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty();

        RuleFor(command => command.PositionId)
            .NotEmpty();
    }
}