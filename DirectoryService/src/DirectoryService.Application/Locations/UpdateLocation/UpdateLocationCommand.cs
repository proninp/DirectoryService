using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.UpdateLocation;

public record UpdateLocationCommand(Guid Id, UpdateLocationRequest Request) : IValidationCommand
{
    public object LogContext => new { Id, Request };
}

public static class UpdateLocationCommandExtensions
{
    public static UpdateLocationCommand ToCommand(this UpdateLocationRequest request, Guid id) =>
        new(id, request);
}