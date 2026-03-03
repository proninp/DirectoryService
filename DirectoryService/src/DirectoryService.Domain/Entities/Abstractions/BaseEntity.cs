namespace DirectoryService.Domain.Entities.Abstractions;

public abstract class BaseEntity : IEntity<Guid>
{
    public Guid Id { get; init; }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    protected BaseEntity()
    {
    }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; private set; }

    public bool IsActive { get; private set; }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }

        return ReferenceEquals(this, obj) || Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
        Equals(left, right);

    public static bool operator !=(BaseEntity? left, BaseEntity? right) =>
        !Equals(left, right);

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