using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
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

    public async Task<Location?> GetByName(string name, CancellationToken cancellationToken = default)
    {
        var query = context.Locations.AsNoTracking();
        var location = await query.FirstOrDefaultAsync(
            l => EF.Functions.ILike(l.Name, name), cancellationToken);
        return location;
    }

    public async Task<Location?> GetByAddress(Address address, CancellationToken cancellationToken = default)
    {
        var location = await context
            .Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l =>
                l.Address != null &&
                l.Address.PostalCode == address.PostalCode &&
                l.Address.Country == address.Country &&
                l.Address.City == address.City &&
                l.Address.Street == address.Street &&
                l.Address.House == address.House &&
                l.Address.Block == address.Block &&
                l.Address.Room == address.Room &&
                l.Address.PostalBox == address.PostalBox,
                cancellationToken);
        return location;
    }

    public async Task<IReadOnlyList<Location>> GetAll(CancellationToken cancellationToken = default)
    {
        var query = context.Locations.AsNoTracking();
        var locations = await query.ToListAsync(cancellationToken);
        return locations;
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