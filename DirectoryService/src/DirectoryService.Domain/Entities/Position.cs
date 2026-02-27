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

    public string Name { get; set; }

    public string? Description { get; set; }

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

        return Create(name, description);
    }
}