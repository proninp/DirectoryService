namespace DirectoryService.Contracts.Departments.Responses;

public record DepartmentListItemResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string Path,
    DateTime CreatedAt
);