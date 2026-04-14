using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.GetLocation;

public record GetLocationCommand(Guid Id) : ICommand;