using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions.CreatePosition;

public sealed class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreatePositionRequest)));

        RuleFor(command => command.Request.Name)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreatePositionRequest.Name)));

        RuleFor(command => command.Request.DepartmentIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreatePositionRequest.DepartmentIds)));

        RuleFor(command => command.Request.DepartmentIds)
            .Must(ids => ids.Count > 0)
            .WithError(Error.Validation(
                "position.department", "Position departments must contain at least one department.",
                nameof(CreatePositionRequest.DepartmentIds)));

        RuleFor(command => command.Request.DepartmentIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithError(Error.Validation("departmentIds.must.be.unique", "Department Ids must be unique.",
                nameof(CreatePositionRequest.DepartmentIds)));

        RuleForEach(command => command.Request.DepartmentIds)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid("Department Id"));

        RuleFor(command => command.Request)
            .MustBeValueObject(request => Position.Create(
                Guid.NewGuid(), request.Name, request.Description));
    }
}