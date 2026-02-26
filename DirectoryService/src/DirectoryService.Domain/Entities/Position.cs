using DirectoryService.Domain.Entities.Abstractions;

namespace DirectoryService.Domain.Entities;

public sealed class Position(string? description) : AuditableEntity(Guid.NewGuid())
{
    private List<Department> _departments = [];

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = description;

    public IReadOnlyCollection<Department> Departments => _departments;
}