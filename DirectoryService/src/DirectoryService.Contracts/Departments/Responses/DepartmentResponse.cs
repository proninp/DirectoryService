namespace DirectoryService.Contracts.Departments.Responses;

public record DepartmentResponse(string Name, string Identifier, Guid? ParentId, string Path, int Depth);