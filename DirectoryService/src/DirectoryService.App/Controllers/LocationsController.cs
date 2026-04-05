using Asp.Versioning;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations.Requests;
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
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}