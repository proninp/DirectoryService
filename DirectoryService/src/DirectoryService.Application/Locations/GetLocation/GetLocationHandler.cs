using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations.GetLocation;

public sealed class GetLocationHandler(ILocationRepository locationRepository)
    : ICommandHandler<LocationResponse, GetLocationCommand>
{
    public async Task<Result<LocationResponse, Errors>> Handle(
        GetLocationCommand command,
        CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetById(command.Id, cancellationToken);
        if (location is null)
            return GeneralError.NotFound(command.Id, nameof(Location)).ToErrors();

        return location.ToResponse();
    }
}