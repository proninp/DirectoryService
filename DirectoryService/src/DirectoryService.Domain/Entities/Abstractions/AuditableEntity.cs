namespace DirectoryService.Domain.Entities.Abstractions;

public abstract class AuditableEntity : BaseEntity
{
    protected AuditableEntity(Guid id)
        : base(id)
    {
    }

    protected AuditableEntity()
    {
    }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; private set; }

    public bool IsActive { get; private set; }

    public virtual void Delete()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        DeletedAt = null;
    }
}