using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.CreateDepartmentPosition;

public record CreateDepartmentPositionCommand(Guid DepartmentId, Guid PositionId) : IValidationCommand
{
    public object? LogContext => new { DepartmentId, PositionId };
}