using DirectoryService.Shared;
using FluentValidation.Results;

namespace DirectoryService.Application.Validation;

internal static class ValidationExtensions
{
    public static Errors ToErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .SelectMany(error =>
                Errors.Deserialize(error.ErrorMessage) ?? Enumerable.Empty<Error>())
            .ToList();
    }
}