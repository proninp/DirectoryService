using System.Net;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.EndpointResults;

public static class EndpointResultErrorExtensions
{
    public static ObjectResult ToObjectResult(this Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(error),
            ErrorType.Validation => new BadRequestObjectResult(error),
            ErrorType.Conflict => new ConflictObjectResult(error),
            _ => new ObjectResult(error) { StatusCode = (int)HttpStatusCode.InternalServerError }
        };
    }
}