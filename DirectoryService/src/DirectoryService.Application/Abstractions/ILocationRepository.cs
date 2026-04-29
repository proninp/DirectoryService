using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Application.Abstractions;

public interface ILocationRepository
{
    Task<Location?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<Location?> GetByName(string name, CancellationToken cancellationToken = default);

    Task<Location?> GetByAddress(Address address, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Location>> GetAll(CancellationToken cancellationToken = default);

    Task<Guid> Add(Location location, CancellationToken cancellationToken = default);

    Task Update(Location location, CancellationToken cancellationToken = default);

    Task<bool> Delete(Location location, CancellationToken cancellationToken = default);
}