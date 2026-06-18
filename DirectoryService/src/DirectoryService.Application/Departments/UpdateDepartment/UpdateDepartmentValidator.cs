using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public sealed class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentRequest)))
            .DependentRules(() =>
            {
                RuleFor(command => command.Id)
                    .Must(id => id != Guid.Empty)
                    .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentCommand.Id)));

                RuleFor(command => command.Request)
                    .Must(r =>
                        !string.IsNullOrWhiteSpace(r.Name) || r.LocationIds is { Count: > 0 })
                    .WithError(Error.Validation(
                        "department.location.must.be.specified",
                        "Either Name or LocationIds must be provided.",
                        nameof(UpdateDepartmentRequest)));

                When(command => command.Request.Name is not null, () =>
                {
                    RuleFor(command => command.Request.Name)
                        .Must(name => !string.IsNullOrWhiteSpace(name))
                        .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateDepartmentRequest.Name)));
                });

                When(command => command.Request.LocationIds is not null, () =>
                {
                    RuleFor(command => command.Request.LocationIds)
                        .Must(ids => ids!.Count > 0)
                        .WithError(Error.Validation(
                            "department.location", "Department locations must contain at least one location.",
                            nameof(UpdateDepartmentRequest.LocationIds)));

                    RuleFor(command => command.Request.LocationIds)
                        .Must(ids => ids!.Distinct().Count() == ids!.Count)
                        .WithError(Error.Validation(
                            "locationIds.must.be.unique", "Location Ids must be unique.",
                            nameof(UpdateDepartmentRequest.LocationIds)));

                    RuleForEach(command => command.Request.LocationIds)
                        .Must(id => id != Guid.Empty)
                        .WithError(GeneralErrors.ValueIsInvalid("Location Id"));
                });
            });
    }
}