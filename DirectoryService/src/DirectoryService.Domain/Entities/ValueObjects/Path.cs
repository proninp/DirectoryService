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

    public static Result<Path, Errors> CreateForParent(string path)
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

    public static Result<Path, Errors> CreateForChild(Path parentPath, Slug childSlug) =>
        CreateForParent($"{parentPath.Value}{Separator}{childSlug.Value}");

    public static Result<Path, Errors> FromAncestors(
        IReadOnlyList<Department> ancestors,
        Slug departmentSlug)
    {
        if (ancestors.Count == 0)
        {
            return GeneralErrors.NotFound(
                    recordName: nameof(Path), message: $"No path found for ancestors of {departmentSlug}")
                .ToErrors();
        }

        return CreateForChild(ancestors[^1].Path, departmentSlug);
    }

    public static Result<Path, Errors> Root(Slug slug)
    {
        return CreateForParent(slug.Value);
    }
}