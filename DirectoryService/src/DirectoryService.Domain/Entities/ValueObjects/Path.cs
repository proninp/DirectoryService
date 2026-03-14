using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;

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

    private static Result<Path> Create(string path)
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

        return new Path(path);
    }

    public static Result<Path> CreateForChild(Path parentPath, Identifier childIdentifier)
    {
        var fullPath = $"{parentPath.Value}{Separator}{childIdentifier.Value}";

        return Create(fullPath);
    }

    public static Result<Path> FromAncestors(IReadOnlyList<Department> ancestors, Identifier departmentIdentifier)
    {
        if (ancestors.Count == 0)
            return Result.Failure<Path>($"No path found for ancestors of {departmentIdentifier}");

        return CreateForChild(ancestors[^1].Path, departmentIdentifier);
    }

    public static Result<Path> Root(Identifier departmentIdentifier)
    {
        return Create(departmentIdentifier.Value);
    }
}