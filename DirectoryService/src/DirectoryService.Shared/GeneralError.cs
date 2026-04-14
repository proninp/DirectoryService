namespace DirectoryService.Shared;

public static class GeneralError
{
    public static Error ValueIsInvalid(string? name = null, string? message = null)
    {
        var label = name ?? "Value";
        var errorMessage = message ?? $"{label} is invalid";
        return Error.Validation("value.is.invalid", errorMessage, name);
    }

    public static Error Failure(string recordName, string message)
    {
        return Error.Failure("record.failure", message);
    }

    public static Error InvalidFieldLength(string name, int? minLength = null, int? maxLength = null)
    {
        var message = (minLength.HasValue, maxLength.HasValue) switch
        {
            (true, true) => $"{name} must be between {minLength} and {maxLength} characters.",
            (true, false) => $"{name} must be at least {minLength} characters.",
            (false, true) => $"{name} must not exceed {maxLength} characters.",
            (false, false) => $"{name} has invalid field length."
        };

        return Error.Validation("record.of.invalid.length", message, name);
    }

    public static Error NotFound(Guid? id = null, string? recordName = null, string? message = null)
    {
        var errorMessage = message ?? (id.HasValue
            ? $"{recordName ?? "record"} with id {id.Value} was not found."
            : $"{recordName ?? "record"} was not found.");
        return Error.NotFound("record.not.found", errorMessage);
    }

    public static Error ValueIsRequired(string name)
    {
        return Error.Validation("value.is.required", $"The '{name}' field is required", name);
    }

    public static Error AlreadyExists(Guid? id = null, string? message = null)
    {
        var errorMessage = message ??
                           (id.HasValue ? $"Record already exists with id {id}." : "Record already exists.");
        return Error.Conflict("record.already.exists", errorMessage);
    }

    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Something went wrong. Unexpected server error occurs.");
    }

    public static Error FailureWithNoErrors(string? message = null)
    {
        return Error.Failure(
            "server.failure.no.errors", message ?? "Something went wrong. Unexpected server error occurs.");
    }
}