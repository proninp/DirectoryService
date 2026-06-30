using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository : IRepository
{
    Task<Department?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<Department?> GetByName(string name, CancellationToken cancellationToken = default);

    Task<Department?> GetByIdForUpdate(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlug(Slug slug, CancellationToken cancellationToken = default);

    Task<bool> AllExists(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetAll(CancellationToken cancellationToken = default);

    Guid Add(Department department);

    void Delete(Department department);
}