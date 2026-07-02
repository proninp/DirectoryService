using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Abstractions.Database;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentsQuery { get; }

    IQueryable<Position> PositionsQuery { get; }

    IQueryable<Location> LocationsQuery { get; }
}