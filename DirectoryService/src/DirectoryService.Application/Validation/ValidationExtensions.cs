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

    public static Errors ToErrors(this IEnumerable<ValidationResult> validationResults)
    {
        return validationResults.SelectMany(r => r.Errors)
            .SelectMany(error =>
                Errors.Deserialize(error.ErrorMessage) ?? Enumerable.Empty<Error>())
            .ToList();
    }
}