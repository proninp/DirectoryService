using DirectoryService.Application.Departments;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class DepartmentRepository(DirectoryServiceDbContext context) : IDepartmentRepository
{
    public async Task<Department?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await context
            .Departments
            .Include(d => d.DepartmentLocations)
            .Include(d => d.DepartmentPositions)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        return department;
    }

    public async Task<Department?> GetByName(string name, CancellationToken cancellationToken = default)
    {
        var department = await context
            .Departments
            .Include(d => d.DepartmentLocations)
            .Include(d => d.DepartmentPositions)
            .FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
        return department;
    }

    public async Task<bool> ExistsBySlug(Slug slug, CancellationToken cancellationToken = default)
    {
        return await context
            .Departments
            .AnyAsync(
                d => EF.Functions.ILike(d.Slug.Value, slug.Value),
                cancellationToken);
    }

    public async Task<bool> AllExists(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return true;

        var uniqueIds = ids.Distinct().ToHashSet();

        var existingCount = await context
            .Departments
            .Where(d => uniqueIds.Contains(d.Id))
            .CountAsync(cancellationToken);
        return existingCount == uniqueIds.Count;
    }

    public async Task<IReadOnlyList<Department>> GetAll(CancellationToken cancellationToken = default)
    {
        var departments = await context
            .Departments.AsNoTracking()
            .ToListAsync(cancellationToken);
        return departments;
    }

    public async Task<Guid> Add(Department department, CancellationToken cancellationToken = default)
    {
        context.Add(department);
        await context.SaveChangesAsync(cancellationToken);
        return department.Id;
    }

    public async Task Update(Department department, CancellationToken cancellationToken = default)
    {
        context.Update(department);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> Delete(Department department, CancellationToken cancellationToken = default)
    {
        context.Departments.Remove(department);
        var result = await context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}