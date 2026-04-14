using Asp.Versioning;
using DirectoryService.App.EndpointResults;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.GetLocation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Locations.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.Controllers;

[ApiController]
[ApiVersion(1.0)]
public sealed class LocationsController : ControllerBase
{
    [HttpPost(ApiEndpoints.Locations.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(request.ToCreateCommand(), cancellationToken);
        return result.IsSuccess
            ? new SuccessCreatedResult<Guid>(nameof(Get), new { id = result.Value }, result.Value)
            : new ErrorResult(result.Error);
    }

    [HttpGet(ApiEndpoints.Locations.Get)]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<EndpointResult<LocationResponse>> Get(
        [FromServices] IQueryHandler<LocationResponse, GetLocationQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationQuery(id);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet(ApiEndpoints.Locations.GetAll)]
    [ProducesResponseType(typeof(IReadOnlyList<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<EndpointResult<IReadOnlyList<LocationResponse>>> GetAll(
        [FromServices] IQueryHandler<IReadOnlyList<LocationResponse>, GetLocationsQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery();
        return await handler.Handle(query, cancellationToken);
    }
}