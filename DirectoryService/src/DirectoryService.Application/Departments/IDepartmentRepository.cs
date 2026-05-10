using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository : IRepository
{
    Task<Department?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<Department?> GetByName(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetAll(CancellationToken cancellationToken = default);

    Task<Guid> Add(Department department, CancellationToken cancellationToken = default);

    Task Update(Department department, CancellationToken cancellationToken = default);

    Task<bool> Delete(Department department, CancellationToken cancellationToken = default);
}