using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public interface ICreateLocationHandler
{
    Task<Result<Guid, Errors>> Handle(CreateLocationRequest request, CancellationToken cancellationToken);
}