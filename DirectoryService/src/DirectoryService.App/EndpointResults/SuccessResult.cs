namespace DirectoryService.App.EndpointResults;

public sealed class SuccessResult<TValue>(TValue value) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var envelope = Envelope.Ok(value);

        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}