using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.CreateDepartmentLocation;

public record CreateDepartmentLocationCommand(Guid DepartmentId, Guid LocationId) : IValidationCommand
{
    public object? LogContext => new { DepartmentId, LocationId };
}