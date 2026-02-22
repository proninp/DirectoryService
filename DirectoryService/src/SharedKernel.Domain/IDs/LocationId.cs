namespace SharedKernel.Domain.IDs;

public sealed record LocationId : EntityId<LocationId>
{
    internal LocationId(Guid value)
        : base(value)
    {
    }

    public static LocationId Create() => new(Guid.NewGuid());

    public static LocationId Create(Guid id) => new(id);
}