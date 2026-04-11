using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Path
{
    private const int MinLength = 2;

    private const int MaxLength = 500;

    private const char Separator = '/';

    public string Value { get; } = null!;

    // EF Core Constructor
    private Path() { }

    private Path(string value) => Value = value;

    private static Result<Path, Error> Create(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            path = Regex.Replace(path.Trim(), @"\s+", " ");
        }

        var validationResult = Guard.ValidateStringField(path, nameof(path), MinLength, MaxLength);
        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        return new Path(path);
    }

    public static Result<Path, Error> CreateForChild(Path parentPath, Identifier childIdentifier)
    {
        var fullPath = $"{parentPath.Value}{Separator}{childIdentifier.Value}";

        return Create(fullPath);
    }

    public static Result<Path, Error> FromAncestors(
        IReadOnlyList<Department> ancestors,
        Identifier departmentIdentifier)
    {
        if (ancestors.Count == 0)
        {
            return GeneralError.NotFound(
                recordName: nameof(Path), message: $"No path found for ancestors of {departmentIdentifier}");
        }

        return CreateForChild(ancestors[^1].Path, departmentIdentifier);
    }

    public static Result<Path, Error> Root(Identifier departmentIdentifier)
    {
        return Create(departmentIdentifier.Value);
    }
}