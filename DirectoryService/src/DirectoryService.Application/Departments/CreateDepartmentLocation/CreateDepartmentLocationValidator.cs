using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartmentLocation;

public sealed class CreateDepartmentLocationValidator : AbstractValidator<CreateDepartmentLocationCommand>
{
    public CreateDepartmentLocationValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty();

        RuleFor(command => command.LocationId)
            .NotEmpty();
    }
}