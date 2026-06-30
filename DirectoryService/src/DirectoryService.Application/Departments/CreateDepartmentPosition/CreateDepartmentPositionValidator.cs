using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartmentPosition;

public sealed class CreateDepartmentPositionValidator : AbstractValidator<CreateDepartmentPositionCommand>
{
    public CreateDepartmentPositionValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty();

        RuleFor(command => command.PositionId)
            .NotEmpty();
    }
}