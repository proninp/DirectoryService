using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;

namespace DirectoryService.Shared;

public sealed class Errors : IEnumerable<Error>, ICombine
{
    private readonly IReadOnlyList<Error> _errors;

    [JsonConstructor]
    public Errors(IReadOnlyList<Error> errors) => _errors = [..errors];

    public Errors(Error error) => _errors = [error];

    public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator Errors(Error error) => new([error]);

#pragma warning disable CA1002
    public static implicit operator Errors(List<Error> errors) => new([..errors]);

    public static Errors? Deserialize(string json)
    {
        // System.Text.Json не умеет десериализовать JSON-массив в кастомный тип IEnumerable<T>
        // без метода Add или реализации ICollection<T>
        var list = JsonSerializer.Deserialize<List<Error>>(json, SharedJsonOptions.JsonOptions);
        return list is null ? null : new Errors(list);
    }

    public ICombine Combine(ICombine value)
    {
        if (value is Errors errors)
            return new Errors(this.Concat(errors).ToList());

        if (value is Error error)
            return new Errors(this.Concat([error]).ToList());

        return this;
    }

    public string Serialize() => JsonSerializer.Serialize(this, SharedJsonOptions.JsonOptions);
}