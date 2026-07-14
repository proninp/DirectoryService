using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetLocationListValidator : AbstractValidator<GetLocationListQuery>
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
        { nameof(Location.Name), nameof(Location.CreatedAt), nameof(GetLocationListQuery.Request.MinDepartmentCount) };

    public GetLocationListValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetLocationListQuery.Request)))
            .DependentRules(() =>
            {
                RuleFor(q => q.Request.Search)
                    .MaximumLength(Location.NameMaxLength)
                    .WithError(GeneralErrors.InvalidFieldLength(
                        nameof(GetLocationListRequest.Search), maxLength: Location.NameMaxLength));

                RuleFor(query => query.Request.SortBy)
                    .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) ||
                                    AllowedSortBy.Contains(sortBy))
                    .WithError(GeneralErrors.ValueIsInvalid(nameof(GetLocationListRequest.SortBy)));

                RuleFor(query => query.Request.SortDir)
                    .Must(sortDir => string.IsNullOrWhiteSpace(sortDir) ||
                                     CustomValidators.AllowedSortDir.Contains(sortDir))
                    .WithError(GeneralErrors.ValueIsInvalid(nameof(GetLocationListRequest.SortDir)));

                RuleFor(query => query.Request.Pagination)
                    .NotNull()
                    .WithError(GeneralErrors.ValueIsRequired(nameof(GetLocationListRequest.Pagination)))
                    .SetValidator(new PageRequestValidator());
            });
    }
}