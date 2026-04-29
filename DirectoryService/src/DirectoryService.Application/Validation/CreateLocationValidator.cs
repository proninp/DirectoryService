using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
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

        RuleFor(c => c.Request.AddressRequest)
            .MustBeValueObject(a => a.ToAddress());
    }
}