using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.UpdateLocation;

public sealed class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(command => command.Id)
            .Must(id => id != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateLocationCommand.Id)));

        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateLocationRequest)))
            .DependentRules(() =>
            {
                RuleFor(command => command.Request)
                    .Must(request => !string.IsNullOrWhiteSpace(request.Name) ||
                                     request.AddressRequest != null ||
                                     request.Timezone != null)
                    .WithError(Error.Validation(
                        "update.location.must.be.specified",
                        "Either Name, Address or TimeZone must be provided.",
                        nameof(UpdateLocationRequest)));

                When(command => command.Request.Name != null, () =>
                {
                    RuleFor(command => command.Request.Name)
                        .NotEmpty()
                        .WithError(GeneralErrors.ValueIsRequired(nameof(UpdateLocationCommand.Request.Name)));
                });

                When(command => command.Request.AddressRequest != null, () =>
                {
                    RuleFor(command => command.Request.AddressRequest!)
                        .Must(addressRequest => addressRequest.IsAnyFilled())
                        .WithError(Error.Validation(
                            "update.location.address.must.be.specified",
                            "Either one of the Address fields in Update request must be provided.",
                            nameof(UpdateLocationAddressRequest)));
                });

                When(command => command.Request.Timezone != null, () =>
                {
                    RuleFor(command => command.Request.Timezone!)
                        .MustBeValueObject(Timezone.Create);
                });
            });
    }
}