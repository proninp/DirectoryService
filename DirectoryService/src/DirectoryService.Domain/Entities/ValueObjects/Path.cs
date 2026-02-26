using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Path
{
    private const int MinLength = 2;

    private const int MaxLength = 500;

    private const char Separator = '.';

    public string Value { get; }

    private Path(string value) => Value = value;

    public static Result<Path> Create(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            path = Regex.Replace(path.Trim(), @"\s+", " ");
        }

        var validationResult = Guard.ValidateStringField(path, nameof(path), MinLength, MaxLength);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Path>(validationResult.Error);
        }

        path = new string([.. path.Select(c => char.IsPunctuation(c) ? '_' : c)]);

        return new Path(path);
    }

    public static Result<Path> CreateForChild(Path parentPath, Identifier childIdentifier)
    {
        var fullPath = $"{parentPath.Value}{Separator}{childIdentifier.Value}";

        return Create(fullPath);
    }
}