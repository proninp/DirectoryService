namespace SharedKernel.Domain.IDs;

public record PositionId : EntityId<PositionId>
{
    internal PositionId(Guid value)
        : base(value)
    {
    }

    public static PositionId Create() => new(Guid.NewGuid());

    public static PositionId Create(Guid id) => new(id);
}