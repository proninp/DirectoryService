using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations;

public interface ICreateLocationHandler
{
    Task<Result<Guid>> Handle(CreateLocationRequest request, CancellationToken cancellationToken);
}