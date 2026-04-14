using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.EndpointResults;

public sealed class SuccessCreatedResult<TValue>(string actionName, object? routeValues, TValue value)
    : CreatedAtActionResult(actionName, controllerName: null, routeValues, Envelope.Ok(value));