using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public sealed class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(command => command.Id)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentCommand.Id)));

        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentRequest)))
            .DependentRules(() =>
            {
                RuleFor(command => command.Request.Name)
                    .NotNull()
                    .NotEmpty()
                    .WithError(Error.Validation(
                        "department.location.must.be.specified",
                        "Either Name or LocationIds must be provided.",
                        nameof(UpdateDepartmentRequest)));
            });
    }
}