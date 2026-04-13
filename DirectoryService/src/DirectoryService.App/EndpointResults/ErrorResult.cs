using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.App.EndpointResults;

public sealed class ErrorResult : ObjectResult
{
    public ErrorResult(Errors errors)
        : base(null)
    {
        if (!errors.Any())
        {
            StatusCode = StatusCodes.Status500InternalServerError;
            Value = Envelope.Error(GeneralError.FailureWithNoErrors());
            return;
        }

        var distinctTypes = errors.Select(e => e.ErrorType).Distinct().ToList();
        StatusCode = distinctTypes.Count == 1
            ? (int)distinctTypes[0].ToHttpStatusCode()
            : StatusCodes.Status500InternalServerError;
        Value = Envelope.Error(errors);
    }

    public ErrorResult(Error error)
        : this(new Errors(error))
    {
    }
}