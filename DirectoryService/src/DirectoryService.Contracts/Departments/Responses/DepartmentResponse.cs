namespace DirectoryService.Contracts.Departments.Responses;

public record DepartmentResponse(
    Guid Id,
    string Name,
    string Identifier,
    Guid? ParentId,
    string Path,
    int Depth,
    IReadOnlyCollection<Guid> LocationIds
);