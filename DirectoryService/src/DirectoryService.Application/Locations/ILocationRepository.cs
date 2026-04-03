using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository
{
    Task<Location> GetLocation(Guid id);

    Task<IEnumerable<Location>> GetLocations();

    Task<Guid> CreateLocation(Location location);

    Task UpdateLocation(Location location);

    Task<bool> DeleteLocation(Location location);
}