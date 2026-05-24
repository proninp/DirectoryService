namespace DirectoryService.Contracts.Positions.Responses;

public record PositionResponse(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyCollection<Guid> DepartmentIds);