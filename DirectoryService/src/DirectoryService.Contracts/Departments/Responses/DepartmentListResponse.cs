namespace DirectoryService.Contracts.Departments.Responses;

public record DepartmentListResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string Path,
    DateTime CreatedAt
);