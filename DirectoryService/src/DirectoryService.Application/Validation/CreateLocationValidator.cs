using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Validation;

public sealed class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralError.ValueIsRequired(nameof(CreateLocationCommand.Request)));

        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralError.ValueIsRequired(nameof(CreateLocationCommand.Request.AddressRequest)));

        RuleFor(command => command.Request.AddressRequest)
            .MustBeValueObject(a => a.ToAddress());

        RuleFor(command => command.Request.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}