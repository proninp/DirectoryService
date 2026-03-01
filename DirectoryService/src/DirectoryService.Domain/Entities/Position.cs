using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;

namespace DirectoryService.Domain.Entities;

public sealed class Position : AuditableEntity
{
    private const int NameMinLength = 3;

    private const int NameMaxLength = 100;

    private const int DescriptionMaxLength = 1000;

    private List<Department> _departments = [];

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<Department> Departments => _departments;

    private Position(string name, string? description)
        : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
    }

    public static Result<Position> Create(string name, string? description)
    {
        var validationResult = Result.Combine(
            Guard.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength),
            string.IsNullOrEmpty(description)
                ? Result.Success()
                : Guard.ValidateStringField(description, nameof(Description), 0, DescriptionMaxLength)
        );

        if (validationResult.IsFailure)
            return Result.Failure<Position>(validationResult.Error);

        return new Position(name, description);
    }

    public Result Rename(string newName)
    {
        var validation = Guard.ValidateStringField(newName, nameof(Name), NameMinLength, NameMaxLength);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Error);
        }

        Name = newName;
        return Result.Success();
    }

    public Result Describe(string? newDescription)
    {
        if (newDescription is not null)
        {
            var validation = Guard.ValidateStringField(newDescription, nameof(Description), 0, DescriptionMaxLength);
            if (validation.IsFailure)
            {
                return Result.Failure(validation.Error);
            }
        }

        Description = newDescription;
        return Result.Success();
    }
}