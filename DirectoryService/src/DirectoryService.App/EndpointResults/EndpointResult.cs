using CSharpFunctionalExtensions;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.EndpointResults;

public sealed class EndpointResult<TValue> : IActionResult
{
    private readonly Result<TValue, Error> _result;

    public EndpointResult(Result<TValue, Error> result)
    {
        _result = result;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var actionResult = _result.IsSuccess
            ? new OkObjectResult(_result.Value)
            : _result.Error.ToObjectResult();

        return actionResult.ExecuteResultAsync(context);
    }

    public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result)
    {
        return new EndpointResult<TValue>(result);
    }
}