using Asp.Versioning;
using DirectoryService.App.EndpointResults;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Application.Positions.DeletePosition;
using DirectoryService.Application.Positions.GetPosition;
using DirectoryService.Application.Positions.UpdatePosition;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.Contracts.Positions.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.Controllers;

[ApiController]
[ApiVersion(1.0)]
public sealed class PositionsController : ControllerBase
{
    [HttpGet(ApiEndpoints.Positions.Get)]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<EndpointResult<PositionResponse>> Get(
        [FromServices] IQueryHandler<PositionResponse, GetPositionQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPositionQuery(id);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpPost(ApiEndpoints.Positions.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(request.ToCreateCommand(), cancellationToken);
        return result.IsSuccess
            ? new SuccessCreatedResult<Guid>(nameof(Get), new { id = result.Value }, result.Value)
            : new ErrorResult(result.Error);
    }

    [HttpPatch(ApiEndpoints.Positions.Update)]
    [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<EndpointResult<PositionResponse>> Update(
        [FromServices] ICommandHandler<PositionResponse, UpdatePositionCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request.ToCommand(id), cancellationToken);
    }

    [HttpDelete(ApiEndpoints.Positions.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<Guid, DeletePositionCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeletePositionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : new ErrorResult(result.Error);
    }
}