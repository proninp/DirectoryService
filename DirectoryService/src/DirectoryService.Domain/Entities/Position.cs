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

    public static Result<Position, Errors> Create(string name, string? description)
    {
        var validationResult = Result.Combine(
            Guard.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength),
            string.IsNullOrEmpty(description)
                ? UnitResult.Success<Errors>()
                : Guard.ValidateStringField(description, nameof(Description), 0, DescriptionMaxLength)
        );

        if (validationResult.IsFailure)
            return Result.Failure<Position, Errors>(validationResult.Error);

        return new Position(name, description);
    }

    public UnitResult<Errors> Rename(string newName)
    {
        var validation = Guard.ValidateStringField(newName, nameof(Name), NameMinLength, NameMaxLength);
        if (validation.IsFailure)
            return UnitResult.Failure(validation.Error);

        Name = newName;
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> Describe(string? newDescription)
    {
        if (newDescription is not null)
        {
            var validation =
                Guard.ValidateStringField(newDescription, nameof(Description), 0, DescriptionMaxLength);
            if (validation.IsFailure)
                return UnitResult.Failure(validation.Error);
        }

        Description = newDescription;
        return UnitResult.Success<Errors>();
    }

    internal UnitResult<Errors> AddDepartment(Guid departmentId)
    {
        if (_departmentPositions.All(dp => dp.DepartmentId != departmentId))
        {
            _departmentPositions.Add(new DepartmentPosition(Id, departmentId));
            return UnitResult.Success<Errors>();
        }

        return GeneralError.AlreadyExists(
                departmentId, $"The department '{departmentId}' has already been added to the position.")
            .ToErrors();
    }

    internal UnitResult<Errors> RemoveDepartment(Guid departmentId)
    {
        var removed = _departmentPositions.RemoveAll(dp => dp.DepartmentId == departmentId);
        return removed > 0
            ? UnitResult.Success<Errors>()
            : GeneralError.NotFound(
                    id: departmentId,
                    message: $"There are no departments for the position with the given id: {departmentId}")
                .ToErrors();
    }
}