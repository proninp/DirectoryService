using DirectoryService.Domain.Entities.Abstractions;

namespace DirectoryService.Domain.Entities;

public sealed class DepartmentLocation : BaseEntity
{
    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }

    // EF Core Constructor
    private DepartmentLocation() { }

    public DepartmentLocation(Guid departmentId, Guid locationId)
        : base(Guid.NewGuid())
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
}