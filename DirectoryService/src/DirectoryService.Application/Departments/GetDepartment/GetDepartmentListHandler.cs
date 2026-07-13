using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Contracts.Pagination;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.GetDepartment;

public sealed class GetDepartmentListHandler(
    IValidator<GetDepartmentListQuery> validator,
    IReadDbContext context,
    ILogger<GetDepartmentListHandler> logger
)
    : IQueryHandler<PagedResult<DepartmentListItemResponse>, GetDepartmentListQuery>
{
    private static readonly Dictionary<string, Expression<Func<Department, object>>> SortMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Department.Name)] = d => d.Name,
            [nameof(Department.CreatedAt)] = d => d.CreatedAt
        };

    public async Task<Result<PagedResult<DepartmentListItemResponse>, Errors>> Handle(
        GetDepartmentListQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            logger.LogWarning(
                "Validation failed for {Request}: {@Context} with errors: {@Errors}",
                nameof(query.Request),
                query.Request,
                errors);
            return errors;
        }

        var filteredQuery = context.DepartmentsQuery;

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            filteredQuery = filteredQuery
                .Where(d =>
                    EF.Functions.Like(
                        d.Name.ToUpperInvariant(), $"%{query.Request.Search.ToUpperInvariant()}%"));
        }

        if (string.IsNullOrWhiteSpace(query.Request.SortBy) ||
            !SortMap.TryGetValue(query.Request.SortBy, out var keySelector))
            keySelector = d => d.CreatedAt; // Default sorting

        filteredQuery = query.Request.SortDir?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true
            ? filteredQuery.OrderByDescending(keySelector).ThenByDescending(d => d.Id)
            : filteredQuery.OrderBy(keySelector).ThenBy(d => d.Id);

        var pageSize = query.Request.Pagination.PageSize;
        var pageNumber = query.Request.Pagination.PageNumber;

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var pagedQuery = filteredQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var items = await pagedQuery
            .Select(d => new DepartmentListItemResponse(
                d.Id,
                d.Name,
                d.Slug.Value,
                d.ParentId,
                d.Path.Value,
                d.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentListItemResponse>
        {
            TotalCount = totalCount,
            Items = items.AsReadOnly()
        };
    }
}