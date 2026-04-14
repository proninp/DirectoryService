using System.Net;
using DirectoryService.Shared;

namespace DirectoryService.App.EndpointResults;

public static class EndpointResultErrorExtensions
{
    public static HttpStatusCode ToHttpStatusCode(this ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => HttpStatusCode.NotFound,
            ErrorType.Validation => HttpStatusCode.BadRequest,
            ErrorType.Conflict => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };
    }
}