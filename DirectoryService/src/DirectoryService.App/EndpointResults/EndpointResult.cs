using CSharpFunctionalExtensions;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.EndpointResults;

public sealed class EndpointResult<TValue> : IActionResult
{
    private readonly IActionResult _result;

    public EndpointResult(Result<TValue, Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorResult(result.Error);
    }

    public Task ExecuteResultAsync(ActionContext context) => _result.ExecuteResultAsync(context);

    public static implicit operator EndpointResult<TValue>(Result<TValue, Errors> result) => new(result);
}

public sealed class EndpointResult : IActionResult
{
    private readonly IActionResult _result;

    public EndpointResult(UnitResult<Errors> result)
    {
        _result = result.IsSuccess
            ? new OkObjectResult(Envelope.Ok())
            : new ErrorResult(result.Error);
    }

    public Task ExecuteResultAsync(ActionContext context) => _result.ExecuteResultAsync(context);

    public static implicit operator EndpointResult(UnitResult<Errors> result) => new(result);
}