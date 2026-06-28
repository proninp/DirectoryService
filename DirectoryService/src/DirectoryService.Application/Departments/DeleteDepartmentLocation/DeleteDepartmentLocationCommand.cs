using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.DeleteDepartmentLocation;

public record DeleteDepartmentLocationCommand(Guid DepartmentId, Guid LocationId) : IValidationCommand
{
    public object LogContext => new { DepartmentId, LocationId };
}