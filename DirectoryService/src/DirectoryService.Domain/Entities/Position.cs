using DirectoryService.Domain.Entities.Abstractions;

namespace DirectoryService.Domain.Entities;

public sealed class Position : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}