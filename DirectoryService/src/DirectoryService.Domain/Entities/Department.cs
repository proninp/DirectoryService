using DirectoryService.Domain.Entities.Abstractions;

namespace DirectoryService.Domain.Entities;

public sealed class Department : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Identifier { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public Department? Parent { get; set; }

    public string Path { get; set; } = string.Empty;

    public int Depth { get; set; }
}