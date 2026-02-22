using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Common;

public static class Guard
{
    private static readonly Regex LatinOnlyRegex = new("^[A-Za-z]+$", RegexOptions.Compiled);

    public static Result ValidateStringField(string fieldValue, string fieldName, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(fieldValue))
        {
            return Result.Failure($"{fieldName} is required.");
        }

        if (fieldValue.Length < minLength)
            return Result.Failure($"{fieldName} must be at least {minLength} characters.");

        if (maxLength > 0 && fieldValue.Length > maxLength)
            return Result.Failure($"{fieldName} must not exceed {maxLength} characters.");

        return Result.Success();
    }

    public static Result ValidateLatinString(string fieldValue, string fieldName)
    {
        return !LatinOnlyRegex.IsMatch(fieldValue)
            ? Result.Failure($"{fieldName} must contain only latin letters")
            : Result.Success();
    }
}