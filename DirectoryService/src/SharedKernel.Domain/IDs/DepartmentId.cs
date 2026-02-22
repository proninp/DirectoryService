using System.Runtime.InteropServices;

namespace SharedKernel.Domain.IDs;

public sealed record DepartmentId : EntityId<DepartmentId>
{
    internal DepartmentId(Guid value)
        : base(value)
    {
    }

    public static DepartmentId Create() => new(Guid.NewGuid());

    public static DepartmentId Create(Guid id) => new(id);
}