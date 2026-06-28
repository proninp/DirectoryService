using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions.UpdatePosition;

public sealed class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(command => command.Id)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdatePositionCommand.Id)));

        RuleFor(command => command.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdatePositionRequest)))
            .DependentRules(() =>
            {
                RuleFor(command => command.Request)
                    .Must(request => request.Name != null || request.Description != null)
                    .WithError(Error.Validation(
                        "update.position.must.be.specified",
                        "Either Name or Description must be provided.",
                        nameof(UpdatePositionRequest)));

                When(command => command.Request.Name != null, () =>
                {
                    RuleFor(command => command.Request.Name!)
                        .NotEmpty()
                        .WithError(GeneralErrors.ValueIsRequired(nameof(UpdatePositionRequest.Name)))
                        .DependentRules(() =>
                        {
                            RuleFor(command => command.Request)
                                .MustBeValueObject(request => Position.Create(
                                    Guid.NewGuid(), request.Name!, request.Description));
                        });
                });

                When(command => command.Request.Name == null, () =>
                {
                    RuleFor(command => command.Request.Description!)
                        .MaximumLength(Position.DescriptionMaxLength)
                        .WithError(GeneralErrors.ValueIsInvalid(nameof(UpdatePositionRequest.Description)));
                });
            });
    }
}