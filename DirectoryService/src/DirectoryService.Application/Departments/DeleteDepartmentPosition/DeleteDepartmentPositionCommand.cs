using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.DeleteDepartmentPosition;

public record DeleteDepartmentPositionCommand(Guid DepartmentId, Guid PositionId) : IValidationCommand
{
    public object LogContext => new { DepartmentId, PositionId };
}