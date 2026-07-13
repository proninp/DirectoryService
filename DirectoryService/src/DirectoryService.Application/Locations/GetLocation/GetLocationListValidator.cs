using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Pagination;
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
                    .DependentRules(() =>
                    {
                        RuleFor(query => query.Request.Pagination.PageNumber)
                            .GreaterThanOrEqualTo(1)
                            .WithError(GeneralErrors.ValueIsInvalid(
                                nameof(PagedRequest.PageNumber), "Page number must be greater than or equal to 1."));

                        RuleFor(query => query.Request.Pagination.PageSize)
                            .InclusiveBetween(PagedRequest.MinPageSize, PagedRequest.MaxPageSize)
                            .WithError(GeneralErrors.ValueIsInvalid(
                                nameof(PagedRequest.PageSize),
                                $"Page size must be between {PagedRequest.MinPageSize} and {PagedRequest.MaxPageSize}."));
                    });
            });
    }
}