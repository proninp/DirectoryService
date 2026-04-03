using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations.Requests;

namespace DirectoryService.Application.Locations;

public interface ICreateLocationHandler
{
    Task<Result<Guid, JSType.Error>> Handle(CreateLocationRequest request, CancellationToken cancellationToken);
}