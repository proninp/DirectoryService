namespace DirectoryService.Contracts.Departments.Responses;

public record DepartmentLocationResponse(
    Guid Id,
    string Slug,
    IReadOnlyCollection<Guid> LocationIds);