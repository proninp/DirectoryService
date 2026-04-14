using System.Collections;
using CSharpFunctionalExtensions;

namespace DirectoryService.Shared;

public sealed class Errors : IEnumerable<Error>, ICombine
{
    private readonly List<Error> _errors;

    public Errors(IEnumerable<Error> errors) => _errors = [..errors];

    public Errors(Error error) => _errors = [error];

    public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator Errors(Error error) => new([error]);

#pragma warning disable CA1002
    public static implicit operator Errors(List<Error> errors) => new([..errors]);

    public ICombine Combine(ICombine value)
    {
        if (value is Errors errors)
            return new Errors(this.Concat(errors));

        if (value is Error error)
            return new Errors(this.Concat([error]));

        return this;
    }
}