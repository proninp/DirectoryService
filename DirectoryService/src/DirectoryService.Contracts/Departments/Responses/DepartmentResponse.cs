namespace DirectoryService.Contracts.Departments.Responses;

public record DepartmentResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string Path,
    int Depth,
    IReadOnlyCollection<Guid> LocationIds
);