using SharedKernel.Domain.Entities;
using SharedKernel.Domain.IDs;

namespace DirectoryService.Domain.Entities;

public sealed class Position(PositionId id, string? description) : AuditableEntity<PositionId>(id)
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = description;
}