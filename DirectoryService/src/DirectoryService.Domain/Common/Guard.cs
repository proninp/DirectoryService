using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Common;

internal static class Guard
{
    private static readonly Regex LatinOnlyRegex = new("^[A-Za-z]+$", RegexOptions.Compiled);

    public static UnitResult<Error> ValidateStringField(string fieldValue, string fieldName, int minLength,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(fieldValue))
        {
            return UnitResult.Failure(GeneralError.ValueIsRequired(fieldName));
        }

        if (fieldValue.Length < minLength)
            return UnitResult.Failure(GeneralError.InvalidFieldLength(fieldName, minLength: minLength));

        if (maxLength > 0 && fieldValue.Length > maxLength)
            return UnitResult.Failure(GeneralError.InvalidFieldLength(fieldName, maxLength: maxLength));

        return UnitResult.Success<Error>();
    }

    public static UnitResult<Error> ValidateLatinString(string fieldValue, string fieldName)
    {
        return !LatinOnlyRegex.IsMatch(fieldValue)
            ? UnitResult.Failure(
                GeneralError.ValueIsInvalid(fieldName, $"{fieldName} must contain only latin letters"))
            : UnitResult.Success<Error>();
    }
}