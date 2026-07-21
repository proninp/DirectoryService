using DirectoryService.Application.Positions;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class PositionRepository(DirectoryServiceDbContext context) : IPositionRepository
{
    public async Task<Position?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await context
            .Positions
            .Include(p => p.DepartmentPositions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Position?> GetByIdForUpdate(Guid id, CancellationToken cancellationToken = default)
    {
        await context.Database
            .ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM positions WHERE id = {id}", cancellationToken);

        return await context.Positions
            .Include(p => p.DepartmentPositions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Position?> GetByName(string name, CancellationToken cancellationToken = default)
    {
        return await context
            .Positions
            .Include(p => p.DepartmentPositions)
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Position>> GetAll(CancellationToken cancellationToken = default)
    {
        return await context
            .Positions
            .AsNoTracking()
            .Include(p => p.DepartmentPositions)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> Add(Position position, CancellationToken cancellationToken = default)
    {
        context.Positions.Add(position);
        await context.SaveChangesAsync(cancellationToken);
        return position.Id;
    }

    public void Delete(Position position)
    {
        context.Positions.Remove(position);
    }
}