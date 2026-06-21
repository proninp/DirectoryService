using FluentValidation;

namespace DirectoryService.Application.Departments.DeleteDepartmentLocation;

public sealed class DeleteDepartmentLocationValidator : AbstractValidator<DeleteDepartmentLocationCommand>
{
    public DeleteDepartmentLocationValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty();

        RuleFor(command => command.LocationId)
            .NotEmpty();
    }
}