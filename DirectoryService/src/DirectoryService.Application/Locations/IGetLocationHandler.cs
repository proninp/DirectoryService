using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public interface IGetLocationHandler
{
    Task<Result<LocationResponse, Errors>> Handle(Guid id, CancellationToken cancellationToken);
}