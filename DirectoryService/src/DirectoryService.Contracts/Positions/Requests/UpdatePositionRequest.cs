namespace DirectoryService.Contracts.Positions.Requests;

public record UpdatePositionRequest(
    string? Name,
    string? Description
);