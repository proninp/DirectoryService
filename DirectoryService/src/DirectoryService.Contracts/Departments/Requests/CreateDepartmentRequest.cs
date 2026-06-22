namespace DirectoryService.Contracts.Departments.Requests;

public record CreateDepartmentRequest(
    string Name,
    string Slug,
    Guid? ParentId,
    IReadOnlyCollection<Guid> LocationIds
);