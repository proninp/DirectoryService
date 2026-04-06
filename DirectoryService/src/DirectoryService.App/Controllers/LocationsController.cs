using Asp.Versioning;
using DirectoryService.Application.Locations;
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
        [FromServices] ICreateLocationHandler handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(request, cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(nameof(Get), new { id = result.Value }, result.Value);
    }

    [HttpGet(ApiEndpoints.Locations.Get)]
    [ProducesResponseType(typeof(LocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}