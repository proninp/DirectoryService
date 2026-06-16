namespace DirectoryService.Contracts.Departments.Requests;

public record UpdateDepartmentRequest(
    Guid Id,
    string? Name,
    IReadOnlyCollection<Guid>? LocationIds
);