using Asp.Versioning;
using DirectoryService.App.EndpointResults;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.GetDepartment;
using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Contracts.Departments.Requests;
using DirectoryService.Contracts.Departments.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.Controllers;

[ApiController]
[ApiVersion(1.0)]
public sealed class DepartmentsController : ControllerBase
{
    [HttpPost(ApiEndpoints.Departments.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCreateCommand(), cancellationToken);
        return result.IsSuccess
            ? new SuccessCreatedResult<Guid>(nameof(Get), new { id = result.Value }, result.Value)
            : new ErrorResult(result.Error);
    }

    [HttpGet(ApiEndpoints.Departments.Get)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<EndpointResult<DepartmentResponse>> Get(
        [FromServices] IQueryHandler<DepartmentResponse, GetDepartmentQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentQuery(id);
        return await handler.Handle(query, cancellationToken);
    }

    [HttpPatch(ApiEndpoints.Departments.Update)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    public async Task<EndpointResult<DepartmentResponse>> Update(
        [FromServices] ICommandHandler<DepartmentResponse, UpdateDepartmentCommand> handler,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request.ToUpdateCommand(), cancellationToken);
    }
}