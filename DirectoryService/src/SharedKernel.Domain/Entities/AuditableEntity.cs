using SharedKernel.Domain.IDs;

namespace SharedKernel.Domain.Entities;

public abstract class AuditableEntity<TKey>(TKey id) : BaseEntity<TKey>(id)
    where TKey : EntityId<TKey>
{
    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; private set; }

    public bool IsActive { get; private set; }

    public virtual void Delete()
    {
        if (!IsActive)
            return;
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        if (IsActive)
            return;
        IsActive = true;
        DeletedAt = null;
    }
}