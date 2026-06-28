using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.DeletePosition;

public record DeletePositionCommand(Guid Id) : IValidationCommand
{
    public object LogContext => new { Id };
}