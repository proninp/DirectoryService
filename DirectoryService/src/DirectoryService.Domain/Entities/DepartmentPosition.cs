using DirectoryService.Domain.Entities.Abstractions;

namespace DirectoryService.Domain.Entities;

public sealed class DepartmentPosition : BaseEntity
{
    public Guid DepartmentId { get; private set; }

    public Guid PositionId { get; private set; }

    // EF Core Constructor
    private DepartmentPosition() { }

    public DepartmentPosition(Guid departmentId, Guid positionId)
        : base(Guid.NewGuid())
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}