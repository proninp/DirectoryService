using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Repositories;
using DirectoryService.Domain.Services.Contracts;
using Path = DirectoryService.Domain.Entities.ValueObjects.Path;

namespace DirectoryService.Domain.Services;

/// <summary>
/// Доменный сервис управления иерархией отделов.
/// </summary>
/// <remarks>
/// <para>
/// <b>Создан Domain Service, логика намеренно не вынесена в Application Service.</b><br/>
/// Расчёт <c>Path</c> и <c>Depth</c> - это бизнес логика, а не оркестрация.
/// Правила построения пути и вычисления глубины принадлежат домену и не должны
/// дублироваться в каждом Application Service, который перемещает отдел.
/// </para>
/// <para>
/// <b>Почему сервис зависит от репозитория?</b><br/>
/// <c>Path</c> и <c>Depth</c> - производные от иерархии значения.
/// Их нельзя вычислить, не зная цепочки предков, которая хранится в БД.
/// Сущность <c>Department</c> не может сделать это самостоятельно,
/// поскольку агрегат не имеет доступа к персистентности.
/// </para>
/// </remarks>
internal sealed class DepartmentHierarchyService : IDepartmentHierarchyService
{
    private readonly IDepartmentRepository _repository;

    public DepartmentHierarchyService(IDepartmentRepository repository) => _repository = repository;

    public async Task<Result> MoveAsync(Department department, Guid? newParentId)
    {
        Path newPath;
        int newDepth;

        if (newParentId.HasValue)
        {
            var ancestors = await _repository.GetWithAncestorsAsync(newParentId.Value);
            if (ancestors is null || ancestors.Count == 0)
            {
                return Result.Failure("Parent department not found");
            }

            newDepth = ancestors.Count;
            var pathResult = Path.FromAncestors(ancestors, department.Identifier);
            if (pathResult.IsFailure) return Result.Failure(pathResult.Error);
            newPath = pathResult.Value;
        }
        else
        {
            newDepth = 0;
            var pathResult = Path.Root(department.Identifier);
            if (pathResult.IsFailure) return Result.Failure(pathResult.Error);
            newPath = pathResult.Value;
        }

        return department.MoveTo(newParentId, newPath, newDepth);
    }
}