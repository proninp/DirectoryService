using SharedKernel.Domain.IDs;

namespace SharedKernel.Domain.Entities;

public abstract class BaseEntity<TKey> : IEntity<TKey>
    where TKey : EntityId<TKey>
{
    public TKey Id { get; init; }

    protected BaseEntity(TKey id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TKey> other)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BaseEntity<TKey>? left, BaseEntity<TKey>? right) =>
        Equals(left, right);

    public static bool operator !=(BaseEntity<TKey>? left, BaseEntity<TKey>? right) =>
        !Equals(left, right);
}