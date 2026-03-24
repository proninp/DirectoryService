using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;
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

    public static Result<Department> Create(
        string name, Identifier identifier, Path path, int depth, Guid? parentId
    )
    {
        var validationResult = Result.Combine(
            Guard.ValidateStringField(name, nameof(Name), 3, 150),
            ValidateDepth(depth, parentId)
        );

        if (validationResult.IsFailure) return Result.Failure<Department>(validationResult.Error);

        var department = new Department(name, identifier, path, depth, parentId);
        return Result.Success(department);
    }

    public Result Rename(string newName)
    {
        var nameValidation = Guard.ValidateStringField(newName, nameof(Name), 3, 150);
        if (nameValidation.IsFailure) return Result.Failure(nameValidation.Error);
        Name = newName;
        return Result.Success();
    }

    public Result UpdateIdentifier(Identifier identifier)
    {
        Identifier = identifier;
        return Result.Success();
    }

    public Result MoveTo(Guid? newParentId, Path newPath, int newDepth)
    {
        var validationResult = ValidateDepth(newDepth, newParentId);
        if (validationResult.IsFailure) return Result.Failure(validationResult.Error);

        ParentId = newParentId;
        Path = newPath;
        Depth = newDepth;
        return Result.Success();
    }

    public Result AddLocation(Guid locationId)
    {
        if (_departmentLocations.All(dl => dl.LocationId != locationId))
        {
            _departmentLocations.Add(new DepartmentLocation(Id, locationId));
            return Result.Success();
        }

        return Result.Failure("The location '" + locationId + "' has already been added to the department.");
    }

    public Result RemoveLocation(Guid locationId)
    {
        if (_departmentLocations.Count == 1 && _departmentLocations[0].LocationId == locationId)
            return Result.Failure("Department must have at least one location");

        var removed = _departmentLocations.RemoveAll(dl => dl.LocationId == locationId);
        return removed > 0
            ? Result.Success()
            : Result.Failure("There are no locations in the department with the given id: " + locationId);
    }

    public Result AddPosition(Guid positionId)
    {
        if (_departmentPositions.All(dp => dp.PositionId != positionId))
        {
            _departmentPositions.Add(new DepartmentPosition(Id, positionId));
            return Result.Success();
        }

        return Result.Failure("The position '" + positionId + "' has already been added to the department.");
    }

    public Result RemovePosition(Guid positionId)
    {
        var removed = _departmentPositions.RemoveAll(dp => dp.PositionId == positionId);
        return removed > 0
            ? Result.Success()
            : Result.Failure("There are no positions in the department with the given id: " + positionId);
    }

    private static Result ValidateDepth(int depth, Guid? parentId)
    {
        return (depth < 0
                ? Result.Failure("Depth must be greater than or equal to zero")
                : Result.Success())
            .Bind(() => depth > 0 && !parentId.HasValue
                ? Result.Failure("Department with non zero depth must have at least one parent")
                : Result.Success()
            )
            .Bind(() => depth == 0 && parentId.HasValue
                ? Result.Failure("Department with zero depth can not have a parent")
                : Result.Success()
            );
    }
}