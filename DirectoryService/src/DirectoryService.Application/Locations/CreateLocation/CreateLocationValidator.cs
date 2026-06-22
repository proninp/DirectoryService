using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public sealed class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(CreateLocationRequest)))
            .DependentRules(() =>
            {
                RuleFor(c => c.Request.AddressRequest)
                    .NotNull()
                    .WithError(GeneralErrors.ValueIsRequired(nameof(CreateLocationCommand.Request.AddressRequest)))
                    .DependentRules(() =>
                    {
                        RuleFor(command => command.Request.AddressRequest)
                            .MustBeValueObject(a => a.ToAddress());
                    });

                RuleFor(command => command.Request.Name)
                    .NotEmpty()
                    .WithError(GeneralErrors.ValueIsRequired(nameof(CreateLocationCommand.Request.Name)));

                RuleFor(command => command.Request.Timezone)
                    .MustBeValueObject(Timezone.Create);
            });
    }
}