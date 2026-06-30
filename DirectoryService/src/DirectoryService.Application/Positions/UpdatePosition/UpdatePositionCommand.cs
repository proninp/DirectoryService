using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.UpdatePosition;

public record UpdatePositionCommand(Guid Id, UpdatePositionRequest Request) : IValidationCommand
{
    public object LogContext => new { Id, Request };
}

public static class UpdatePositionCommandExtensions
{
    public static UpdatePositionCommand ToCommand(this UpdatePositionRequest request, Guid id) =>
        new(id, request);
}