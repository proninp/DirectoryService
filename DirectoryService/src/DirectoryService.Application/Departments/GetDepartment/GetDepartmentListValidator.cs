using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.GetDepartment;

public sealed class GetDepartmentListValidator : AbstractValidator<GetDepartmentListQuery>
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
        { nameof(Department.Name), nameof(Department.CreatedAt) };

    public GetDepartmentListValidator()
    {
        RuleFor(q => q.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired(nameof(GetDepartmentListQuery.Request)))
            .DependentRules(() =>
            {
                RuleFor(q => q.Request.Search)
                    .MaximumLength(Department.NameMaxLength)
                    .WithError(GeneralErrors.InvalidFieldLength(
                        nameof(GetDepartmentListRequest.Search), maxLength: Department.NameMaxLength));

                RuleFor(query => query.Request.SortBy)
                    .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) ||
                                    AllowedSortBy.Contains(sortBy))
                    .WithError(GeneralErrors.ValueIsInvalid(nameof(GetDepartmentListRequest.SortBy)));

                RuleFor(query => query.Request.SortDir)
                    .Must(sortDir => string.IsNullOrWhiteSpace(sortDir) ||
                                     CustomValidators.AllowedSortDir.Contains(sortDir))
                    .WithError(GeneralErrors.ValueIsInvalid(nameof(GetDepartmentListRequest.SortDir)));

                RuleFor(query => query.Request.Pagination)
                    .NotNull()
                    .WithError(GeneralErrors.ValueIsRequired(nameof(GetDepartmentListRequest.Pagination)))
                    .SetValidator(new PageRequestValidator());
            });
    }
}