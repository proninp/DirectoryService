using DirectoryService.Contracts.Pagination;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Validation;

public sealed class PageRequestValidator : AbstractValidator<PagedRequest>
{
    public PageRequestValidator()
    {
        RuleFor(pagedRequest => pagedRequest.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.ValueIsInvalid(
                nameof(PagedRequest.PageNumber), "Page number must be greater than or equal to 1."));

        RuleFor(pagedRequest => pagedRequest.PageSize)
            .InclusiveBetween(PagedRequest.MinPageSize, PagedRequest.MaxPageSize)
            .WithError(GeneralErrors.ValueIsInvalid(
                nameof(PagedRequest.PageSize),
                $"Page size must be between {PagedRequest.MinPageSize} and {PagedRequest.MaxPageSize}."));
    }
}