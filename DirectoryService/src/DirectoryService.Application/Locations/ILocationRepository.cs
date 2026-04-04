using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository
{
    Task<Location> GetLocation(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Location>> GetLocations(CancellationToken cancellationToken = default);

    Task<Guid> Create(Location location, CancellationToken cancellationToken = default);

    Task Update(Location location, CancellationToken cancellationToken = default);

    Task<bool> Delete(Location location, CancellationToken cancellationToken = default);
}