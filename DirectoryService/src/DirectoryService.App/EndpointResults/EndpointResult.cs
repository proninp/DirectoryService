using CSharpFunctionalExtensions;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.App.EndpointResults;

public sealed class EndpointResult<TValue> : IResult
{
    private readonly IResult _result;

    public EndpointResult(Result<TValue, Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);

    public static implicit operator EndpointResult<TValue>(Result<TValue, Errors> result) => new(result);
}

public sealed class EndpointResult : IResult
{
    private readonly IResult _result;

    public EndpointResult(UnitResult<Errors> result)
    {
        _result = result.IsSuccess
            ? Results.Ok()
            : new ErrorResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);

    public static implicit operator EndpointResult(UnitResult<Errors> result) => new(result);
}