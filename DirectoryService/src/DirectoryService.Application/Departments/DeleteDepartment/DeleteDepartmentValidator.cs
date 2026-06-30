using FluentValidation;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public sealed class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
