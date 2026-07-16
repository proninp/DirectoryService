using Asp.Versioning;
using DirectoryService.App.EndpointResults;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.DeleteLocation;
using DirectoryService.Application.Locations.GetLocation;
using DirectoryService.Application.Locations.UpdateLocation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Contracts.Locations.Responses;
using DirectoryService.Contracts.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.Controllers;

[ApiController]
[ApiVersion(1.0)]
public sealed class LocationsController : ControllerBase
{
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

    [HttpGet(ApiEndpoints.Locations.GetList)]
    [ProducesResponseType(typeof(PagedResult<LocationListItemResponse>), StatusCodes.Status200OK)]
    public async Task<EndpointResult<PagedResult<LocationListItemResponse>>> Get(
        [FromServices] IQueryHandler<PagedResult<LocationListItemResponse>, GetLocationListQuery> handler,
        [FromQuery] GetLocationListRequest request,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(request.ToQuery(), cancellationToken);
    }

    [HttpGet(ApiEndpoints.Locations.GetTop)]
    [ProducesResponseType(typeof(IReadOnlyList<TopLocationsResponse>), StatusCodes.Status200OK)]
    public async Task<EndpointResult<IReadOnlyList<TopLocationsResponse>>> GetTop(
        [FromServices] IQueryHandler<IReadOnlyList<TopLocationsResponse>, GetLocationsQuery> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery();
        return await handler.Handle(query, cancellationToken);
    }

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

    [HttpPatch(ApiEndpoints.Locations.Update)]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<EndpointResult<LocationResponse>> Update(
        [FromServices] ICommandHandler<LocationResponse, UpdateLocationCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken
    )
    {
        return await handler.Handle(request.ToCommand(id), cancellationToken);
    }

    [HttpDelete(ApiEndpoints.Locations.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<Guid, DeleteLocationCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeleteLocationCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : new ErrorResult(result.Error);
    }
}