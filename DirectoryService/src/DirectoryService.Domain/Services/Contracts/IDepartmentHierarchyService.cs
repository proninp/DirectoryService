using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;

namespace DirectoryService.Domain.Services.Contracts;

public interface IDepartmentHierarchyService
{
    Task<Result> MoveAsync(Department department, Guid? newParentId);
}