using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions.Requests;

namespace DirectoryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : IValidationCommand
{
    public object LogContext => Request;
}

public static class CreatePositionCommandExtensions
{
    public static CreatePositionCommand ToCreateCommand(this CreatePositionRequest request) => new(request);
}