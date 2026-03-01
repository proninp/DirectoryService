using DirectoryService.Domain.Entities;

namespace DirectoryService.Domain.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id);

    /// <summary>
    /// Возвращает цепочку Department, включая самого родителя [root, ..., parent].
    /// </summary>
    /// <param name="departmentId">Department для которого возвращается цепочка предков.</param>
    /// <returns>Неизменяемый список сущностей Department от корня к последнему предку.</returns>
    Task<IReadOnlyList<Department>?> GetWithAncestorsAsync(Guid departmentId);
}