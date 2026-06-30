using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Application.Locations;

public interface ILocationRepository : IRepository
{
    Task<Location?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<Location?> GetByIdForUpdate(Guid id, CancellationToken cancellationToken = default);

    Task<Location?> GetByName(string name, CancellationToken cancellationToken = default);

    Task<Location?> GetByAddress(Address address, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Location>> GetAll(CancellationToken cancellationToken = default);

    Task<bool> AllExists(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Guid Add(Location location);

    void Delete(Location location);
}