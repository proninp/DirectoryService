using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public sealed class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreateDepartmentRequest)))
            .DependentRules(() =>
            {
                RuleFor(command => command.Request.Name)
                    .NotNull()
                    .WithError(GeneralErrors.ValueIsRequired(nameof(CreateDepartmentRequest.Name)));

                RuleFor(command => command.Request.Slug)
                    .MustBeValueObject(Slug.Create);

                RuleFor(x => x.Request.ParentId)
                    .Must(id => id == null || id.Value != Guid.Empty)
                    .WithError(GeneralErrors.ValueIsInvalid(nameof(CreateDepartmentRequest.ParentId)));

                RuleFor(command => command.Request.LocationIds)
                    .NotNull()
                    .WithError(GeneralErrors.ValueIsRequired(nameof(CreateDepartmentRequest.LocationIds)));

                RuleFor(command => command.Request.LocationIds)
                    .Must(list => list is { Count: > 0 })
                    .WithError(Error.Validation(
                        "department.location", "Department locations must contain at least one location.",
                        nameof(CreateDepartmentRequest.LocationIds)));

                RuleFor(command => command.Request.LocationIds)
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithError(Error.Validation(
                        "locationIds.must.be.unique", "Location Ids must be unique.",
                        nameof(CreateDepartmentRequest.LocationIds)));

                RuleForEach(command => command.Request.LocationIds)
                    .Must(id => id != Guid.Empty)
                    .WithError(GeneralErrors.ValueIsInvalid("Location Id"));
            });
    }
}