namespace DirectoryService.Contracts.Departments.Responses;

public record DepartmentPositionResponse(
    Guid Id,
    string Slug,
    IReadOnlyCollection<Guid> PositionIds);
