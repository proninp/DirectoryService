using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Positions;

public interface IPositionRepository : IRepository
{
    Task<Position?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<Position?> GetByName(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Position>> GetAll(CancellationToken cancellationToken = default);

    Task<Guid> Add(Position position, CancellationToken cancellationToken = default);

    Task Update(Position position, CancellationToken cancellationToken = default);

    Task<bool> Delete(Position position, CancellationToken cancellationToken = default);
}