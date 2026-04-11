using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Entities;

public sealed class Position : BaseEntity
{
    private const int NameMinLength = 3;

    public const int NameMaxLength = 100;

    public const int DescriptionMaxLength = 1000;

    private readonly List<DepartmentPosition> _departmentPositions = [];

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public IReadOnlyCollection<DepartmentPosition> DepartmentPositions => _departmentPositions;

    // EF Core Constructor
    private Position() { }

    private Position(string name, string? description)
        : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
    }

    public static Result<Position, Error> Create(string name, string? description)
    {
        var validationResult = Result.Combine(
            Guard.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength),
            string.IsNullOrEmpty(description)
                ? UnitResult.Success<Error>()
                : Guard.ValidateStringField(description, nameof(Description), 0, DescriptionMaxLength)
        );

        if (validationResult.IsFailure)
            return Result.Failure<Position, Error>(validationResult.Error);

        return new Position(name, description);
    }

    public UnitResult<Error> Rename(string newName)
    {
        var validation = Guard.ValidateStringField(newName, nameof(Name), NameMinLength, NameMaxLength);
        if (validation.IsFailure)
            return UnitResult.Failure(validation.Error);

        Name = newName;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Describe(string? newDescription)
    {
        if (newDescription is not null)
        {
            var validation =
                Guard.ValidateStringField(newDescription, nameof(Description), 0, DescriptionMaxLength);
            if (validation.IsFailure)
                return UnitResult.Failure(validation.Error);
        }

        Description = newDescription;
        return UnitResult.Success<Error>();
    }

    internal UnitResult<Error> AddDepartment(Guid departmentId)
    {
        if (_departmentPositions.All(dp => dp.DepartmentId != departmentId))
        {
            _departmentPositions.Add(new DepartmentPosition(Id, departmentId));
            return UnitResult.Success<Error>();
        }

        return GeneralError.AlreadyExists(
            departmentId, $"The department '{departmentId}' has already been added to the position.");
    }

    internal UnitResult<Error> RemoveDepartment(Guid departmentId)
    {
        var removed = _departmentPositions.RemoveAll(dp => dp.DepartmentId == departmentId);
        return removed > 0
            ? UnitResult.Success<Error>()
            : GeneralError.NotFound(
                id: departmentId,
                message: $"There are no departments for the position with the given id: {departmentId}");
    }
}