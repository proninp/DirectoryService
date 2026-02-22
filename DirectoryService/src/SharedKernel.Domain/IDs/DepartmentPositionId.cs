namespace SharedKernel.Domain.IDs;

public sealed record DepartmentPositionId : EntityId<DepartmentPositionId>
{
    internal DepartmentPositionId(Guid value)
        : base(value)
    {
    }

    public static DepartmentPositionId Create() => new(Guid.NewGuid());

    public static DepartmentPositionId Create(Guid id) => new(id);
}