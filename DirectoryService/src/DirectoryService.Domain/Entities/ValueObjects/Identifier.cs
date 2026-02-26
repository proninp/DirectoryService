using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Identifier
{
    private const int MinLength = 3;

    private const int MaxLength = 150;

    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier> Create(string identifier)
    {
        if (!string.IsNullOrEmpty(identifier))
        {
            identifier = Regex.Replace(identifier.Trim(), @"\s+", " ");
        }

        var validationResult = Guard.ValidateStringField(identifier, nameof(Identifier), MinLength, MaxLength)
            .Bind(() => Guard.ValidateLatinString(identifier, nameof(Identifier)));
        if (validationResult.IsFailure)
            return Result.Failure<Identifier>(validationResult.Error);

        return new Identifier(identifier);
    }
}