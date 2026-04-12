using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Identifier
{
    private const int MinLength = 3;

    private const int MaxLength = 150;

    public string Value { get; } = null!;

    // EF Core Constructor
    private Identifier() { }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Errors> Create(string identifier)
    {
        if (!string.IsNullOrEmpty(identifier))
        {
            identifier = Regex.Replace(identifier.Trim(), @"\s+", " ");
            identifier = Regex.Replace(identifier, @"\p{P}", "_");
        }

        var validationResult = Guard.ValidateStringField(identifier, nameof(Identifier), MinLength, MaxLength)
            .Bind(() => Guard.ValidateLatinString(identifier, nameof(Identifier)));
        if (validationResult.IsFailure)
            return validationResult.Error;

        return new Identifier(identifier);
    }
}