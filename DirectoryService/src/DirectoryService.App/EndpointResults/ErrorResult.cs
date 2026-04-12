using DirectoryService.Shared;

namespace DirectoryService.App.EndpointResults;

public sealed class ErrorResult : IResult
{
    private readonly Errors _errors;

    public ErrorResult(Errors errors) => _errors = errors;

    public ErrorResult(Error error) => _errors = error;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!_errors.Any())
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return httpContext.Response.WriteAsJsonAsync(Envelope.Error(GeneralError.FailureWithNoErrors()));
        }

        var distinctTypes = _errors
            .Select(e => e.ErrorType)
            .Distinct()
            .ToList();
        var statusCode = distinctTypes.Count == 1
            ? (int)distinctTypes[0].ToHttpStatusCode()
            : StatusCodes.Status500InternalServerError;

        httpContext.Response.StatusCode = statusCode;
        return httpContext.Response.WriteAsJsonAsync(Envelope.Error(_errors));
    }
}