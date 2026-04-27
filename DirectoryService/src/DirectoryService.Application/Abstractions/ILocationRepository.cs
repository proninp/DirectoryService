using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Abstractions;

public interface ILocationRepository
{
    Task<Location?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<Location?> GetByName(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Location>> GetAll(CancellationToken cancellationToken = default);

    Task<Guid> Add(Location location, CancellationToken cancellationToken = default);

    Task Update(Location location, CancellationToken cancellationToken = default);

    Task<bool> Delete(Location location, CancellationToken cancellationToken = default);
}