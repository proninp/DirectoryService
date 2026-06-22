using System.Globalization;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Slug
{
    private const int MinLength = 3;

    private const int MaxLength = 150;

    public string Value { get; } = null!;

    // EF Core Constructor
    private Slug() { }

    private Slug(string value)
    {
        Value = value;
    }

    public override int GetHashCode() =>
        HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(Value));

    public bool Equals(Slug? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public static Result<Slug, Errors> Create(string slug)
    {
        if (!string.IsNullOrEmpty(slug))
        {
            slug = Regex.Replace(slug.Trim(), @"\s+", " ");
            slug = Regex.Replace(slug, @"[\p{P}-[.]]", "_");
        }

        var validationResult = Guard.ValidateStringField(slug, nameof(Slug), MinLength, MaxLength)
            .Bind(() => Guard.ValidateLatinString(slug, nameof(Slug)));
        if (validationResult.IsFailure)
            return validationResult.Error;

        return new Slug(slug);
    }
}