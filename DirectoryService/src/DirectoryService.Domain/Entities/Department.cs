using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;
using Path = DirectoryService.Domain.Entities.ValueObjects.Path;

namespace DirectoryService.Domain.Entities;

public sealed class Department : AuditableEntity
{
    private List<Department> _children = [];

    private List<Location> _locations = [];

    private List<Position> _positions = [];

    public string Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<Location> Locations => _locations;

    public IReadOnlyList<Position> Positions => _positions;

    public Path Path { get; private set; }

    public int Depth { get; private set; }

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

    public Result AddLocation(Location location)
    {
        if (_locations.All(l => l.Id != location.Id))
            _locations.Add(location);
        return Result.Success();
    }

    public Result RemoveLocation(Guid locationId)
    {
        if (_locations.Count == 1 && _locations.First().Id == locationId)
            return Result.Failure("Department must have at least one location");

        var result = _locations.RemoveAll(l => l.Id == locationId);
        return result > 0
            ? Result.Success()
            : Result.Failure("There are no locations in the department with the given id: " + locationId);
    }

    public Result AddPosition(Position position)
    {
        if (_positions.All(p => p.Id != position.Id))
            _positions.Add(position);
        return Result.Success();
    }

    public Result RemovePosition(Guid positionId)
    {
        var result = _positions.RemoveAll(p => p.Id == positionId);
        return result > 0
            ? Result.Success()
            : Result.Failure("There are no position in the department with the given id: " + positionId);
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