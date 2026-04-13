using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.EndpointResults;

public sealed class SuccessResult<TValue>(TValue value) : OkObjectResult(Envelope.Ok(value));