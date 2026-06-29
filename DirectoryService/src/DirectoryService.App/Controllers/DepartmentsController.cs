using Asp.Versioning;
using DirectoryService.App.EndpointResults;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.CreateDepartmentLocation;
using DirectoryService.Application.Departments.DeleteDepartment;
using DirectoryService.Application.Departments.DeleteDepartmentLocation;
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

    [HttpPatch(ApiEndpoints.Departments.Update)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<EndpointResult<DepartmentResponse>> Update(
        [FromServices] ICommandHandler<DepartmentResponse, UpdateDepartmentCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request.ToCommand(id), cancellationToken);
    }

    [HttpPost(ApiEndpoints.Departments.UpdateDepartmentLocation)]
    [ProducesResponseType(typeof(DepartmentLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<EndpointResult<DepartmentLocationResponse>> CreateDepartmentLocation(
        [FromServices] ICommandHandler<DepartmentLocationResponse, CreateDepartmentLocationCommand> handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentLocationCommand(departmentId, locationId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete(ApiEndpoints.Departments.UpdateDepartmentLocation)]
    [ProducesResponseType(typeof(DepartmentLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<EndpointResult<DepartmentLocationResponse>> DeleteDepartmentLocation(
        [FromServices] ICommandHandler<DepartmentLocationResponse, DeleteDepartmentLocationCommand> handler,
        [FromRoute] Guid departmentId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDepartmentLocationCommand(departmentId, locationId);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete(ApiEndpoints.Departments.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<Guid, DeleteDepartmentCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeleteDepartmentCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : new ErrorResult(result.Error);
    }
}