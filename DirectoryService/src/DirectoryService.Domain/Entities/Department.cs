using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using Path = DirectoryService.Domain.Entities.ValueObjects.Path;

namespace DirectoryService.Domain.Entities;

public sealed class Department : BaseEntity
{
    private List<Department> _children = [];

    private List<DepartmentLocation> _departmentLocations = [];

    private List<DepartmentPosition> _departmentPositions = [];

    public string Name { get; private set; } = null!;

    public Identifier Identifier { get; private set; } = null!;

    public Guid? ParentId { get; private set; }

    public Path Path { get; private set; } = null!;

    public int Depth { get; private set; }

    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    // EF Core Constructor
    private Department() { }

    private Department(
        string name, Identifier identifier, Path path, int depth, Guid? parentId
    )
        : base(Guid.NewGuid())
    {
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
    }

    public static Result<Department, Error> Create(
        string name, Identifier identifier, Path path, int depth, Guid? parentId
    )
    {
        var validationResult = Result.Combine(
            Guard.ValidateStringField(name, nameof(Name), 3, 150),
            ValidateDepth(depth, parentId)
        );

        if (validationResult.IsFailure)
            return Result.Failure<Department, Error>(validationResult.Error);

        var department = new Department(name, identifier, path, depth, parentId);
        return Result.Success<Department, Error>(department);
    }

    public UnitResult<Error> Rename(string newName)
    {
        var nameValidation = Guard.ValidateStringField(newName, nameof(Name), 3, 150);
        if (nameValidation.IsFailure)
        {
            return UnitResult.Failure(nameValidation.Error);
        }

        Name = newName;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateIdentifier(Identifier identifier)
    {
        Identifier = identifier;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MoveTo(Guid? newParentId, Path newPath, int newDepth)
    {
        var validationResult = ValidateDepth(newDepth, newParentId);
        if (validationResult.IsFailure)
        {
            return UnitResult.Failure(validationResult.Error);
        }

        ParentId = newParentId;
        Path = newPath;
        Depth = newDepth;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddLocation(Guid locationId)
    {
        if (_departmentLocations.All(dl => dl.LocationId != locationId))
        {
            _departmentLocations.Add(new DepartmentLocation(Id, locationId));
            return UnitResult.Success<Error>();
        }

        return GeneralError.AlreadyExists(
            locationId, $"The location '{locationId}' has already been added to the department.");
    }

    public UnitResult<Error> RemoveLocation(Guid locationId)
    {
        if (_departmentLocations.Count == 1 && _departmentLocations[0].LocationId == locationId)
            return GeneralError.Failure(nameof(Department), message: "Department must have at least one location");

        var removed = _departmentLocations.RemoveAll(dl => dl.LocationId == locationId);
        return removed > 0
            ? UnitResult.Success<Error>()
            : GeneralError.Failure($"There are no locations in the department with the given id: {locationId}");
    }

    public UnitResult<Error> AddPosition(Guid positionId)
    {
        if (_departmentPositions.All(dp => dp.PositionId != positionId))
        {
            _departmentPositions.Add(new DepartmentPosition(Id, positionId));
            return UnitResult.Success<Error>();
        }

        return GeneralError.AlreadyExists(
            positionId, "The position '" + positionId + "' has already been added to the department.");
    }

    public UnitResult<Error> RemovePosition(Guid positionId)
    {
        var removed = _departmentPositions.RemoveAll(dp => dp.PositionId == positionId);
        return removed > 0
            ? UnitResult.Success<Error>()
            : GeneralError.NotFound(
                id: positionId, message: $"There are no positions in the department with the given id: {positionId}");
    }

    private static UnitResult<Error> ValidateDepth(int depth, Guid? parentId)
    {
        const string fieldName = nameof(Depth);
        if (depth < 0)
        {
            return UnitResult.Failure(
                GeneralError.ValueIsInvalid(fieldName, $"{fieldName} must be greater than or equal to zero"));
        }

        if (depth > 0 && !parentId.HasValue)
        {
            return UnitResult.Failure(
                GeneralError.ValueIsInvalid(fieldName, "Department with non-zero depth must have a parent"));
        }

        if (depth == 0 && parentId.HasValue)
        {
            return UnitResult.Failure(
                GeneralError.ValueIsInvalid(fieldName, "Department with zero depth cannot have a parent"));
        }

        return UnitResult.Success<Error>();
    }
}