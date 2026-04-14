using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;

public static class CreateLocationCommandExtensions
{
    public static CreateLocationCommand ToCreateCommand(this CreateLocationRequest request) =>
        new(request);
}