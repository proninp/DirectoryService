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
            Guard.ValidateStringField(identifier.Value, nameof(Identifier), 3, 150)
                .Bind(() => Guard.ValidateLatinString(identifier.Value, nameof(Identifier))),
            Result.Success()
        );

        if (validationResult.IsFailure)
            return Result.Failure<Department>(validationResult.Error);

        var department = new Department(name, identifier, path, depth, parentId);
        return Result.Success(department);
    }

    public Result AddLocation(Location location)
    {
        if (!_locations.Contains(location))
            _locations.Add(location);
        return Result.Success();
    }

    public Result RemoveLocation(Location location)
    {
        _locations.Remove(location);
        return Result.Success();
    }

    public Result AddPosition(Position position)
    {
        if (!_positions.Contains(position))
            _positions.Add(position);
        return Result.Success();
    }

    public Result RemovePosition(Position position)
    {
        _positions.Remove(position);
        return Result.Success();
    }
}