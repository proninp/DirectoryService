namespace SharedKernel.Domain.IDs;

public abstract record EntityId<TSelf> : IComparable<TSelf>
where TSelf : EntityId<TSelf>
{
    private protected EntityId(Guid value) => Value = value;

    public Guid Value { get; }

    public int CompareTo(TSelf? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);
}