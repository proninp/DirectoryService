using DirectoryService.Application.Locations;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class LocationRepository(DirectoryServiceDbContext context) : ILocationRepository
{
    public async Task<Location?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await context.Locations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return location;
    }

    public async Task<Location?> GetByName(string name, CancellationToken cancellationToken = default)
    {
        var location = await context.Locations
            .FirstOrDefaultAsync(
                l => EF.Functions.ILike(l.Name, name), cancellationToken);
        return location;
    }

    public async Task<Location?> GetByAddress(Address address, CancellationToken cancellationToken = default)
    {
        var location = await context
            .Locations
            .FirstOrDefaultAsync(
                l =>
                    l.Address == address,
                cancellationToken);
        return location;
    }

    public async Task<IReadOnlyList<Location>> GetAll(CancellationToken cancellationToken = default)
    {
        var query = context.Locations.AsNoTracking();
        var locations = await query.ToListAsync(cancellationToken);
        return locations;
    }

    public async Task<bool> AllExists(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return true;
        var existingCount = await context.Locations
            .Where(l => ids.Contains(l.Id))
            .CountAsync(cancellationToken);

        return existingCount == ids.Count;
    }

    public async Task<Guid> Add(Location location, CancellationToken cancellationToken = default)
    {
        context.Locations.Add(location);
        await context.SaveChangesAsync(cancellationToken);
        return location.Id;
    }

    public Task Update(Location location, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<bool> Delete(Location location, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}