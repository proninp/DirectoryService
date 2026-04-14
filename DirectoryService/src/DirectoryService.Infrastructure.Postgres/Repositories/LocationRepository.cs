using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class LocationRepository(DirectoryServiceDbContext context) : ILocationRepository
{
    public async Task<Location?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = context.Locations.AsNoTracking();
        var location = await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return location;
    }

    public Task<IEnumerable<Location>> GetAll(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

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