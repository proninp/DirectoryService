namespace DirectoryService.Domain.Entities.Abstractions;

public abstract class AuditableEntity : BaseEntity
{
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; set; }
}