namespace DirectoryService.Contracts.Departments.Requests;

public record CreateDepartmentRequest(
    string Name,
    string Identifier,
    Guid? ParentId,
    IReadOnlyCollection<Guid> LocationIds
);