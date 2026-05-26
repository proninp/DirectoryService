using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.GetDepartment;

public sealed class GetDepartmentHandler(
    IDepartmentRepository departmentRepository,
    ILogger<GetDepartmentHandler> logger
) : IQueryHandler<DepartmentResponse, GetDepartmentQuery>
{
    public async Task<Result<DepartmentResponse, Errors>> Handle(
        GetDepartmentQuery query, CancellationToken cancellationToken)
    {
        if (query.Id == Guid.Empty)
        {
            logger.LogWarning($"Get {nameof(Department)} query error: query id parameter is empty.");
            return GeneralErrors.ValueIsRequired(nameof(query.Id)).ToErrors();
        }

        var department = await departmentRepository.GetById(query.Id, cancellationToken);

        if (department == null)
        {
            logger.LogWarning(
                "{DepartmentQueryName} query error: department was not found with id: {DepartmentId}.",
                nameof(GetDepartmentQuery),
                query.Id);
            return GeneralErrors.NotFound(
                    query.Id, nameof(Department), $"No active departments were found with id: '{query.Id}'.")
                .ToErrors();
        }

        return department.ToResponse();
    }
}