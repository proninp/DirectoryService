using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Pagination;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.GetDepartment;

public sealed class GetDepartmentListValidator : AbstractValidator<GetDepartmentListQuery>
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
        { nameof(Department.Name), nameof(Department.CreatedAt) };

    private static readonly HashSet<string> AllowedSortDir =
        new(StringComparer.OrdinalIgnoreCase) { "asc", "desc" };

    public GetDepartmentListValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetDepartmentListQuery.Request)))
            .DependentRules(() =>
            {
                RuleFor(q => q.Request.Search)
                    .MaximumLength(Department.DepartmentNameMaxLength)
                    .WithError(GeneralErrors.InvalidFieldLength(
                        nameof(GetDepartmentListRequest.Search), maxLength: Department.DepartmentNameMaxLength));

                RuleFor(query => query.Request.SortBy)
                    .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) ||
                                    AllowedSortBy.Contains(sortBy))
                    .WithError(GeneralErrors.ValueIsInvalid(nameof(GetDepartmentListRequest.SortBy)));

                RuleFor(query => query.Request.SortDir)
                    .Must(sortDir => string.IsNullOrWhiteSpace(sortDir) ||
                                     AllowedSortDir.Contains(sortDir))
                    .WithError(GeneralErrors.ValueIsInvalid(nameof(GetDepartmentListRequest.SortDir)));

                RuleFor(query => query.Request.Pagination)
                    .NotNull()
                    .WithError(GeneralErrors.ValueIsRequired(nameof(GetDepartmentListRequest.Pagination)))
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