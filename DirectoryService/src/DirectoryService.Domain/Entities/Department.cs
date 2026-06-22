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

    public Slug Slug { get; private set; } = null!;

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
        Guid id,
        string name,
        Slug slug,
        Path path,
        int depth,
        Guid? parentId,
        IReadOnlyCollection<DepartmentLocation> departmentLocations)
        : base(id)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        _departmentLocations = [.. departmentLocations];
    }

    public static Result<Department, Errors> Create(string name, Slug slug,
        IReadOnlyCollection<DepartmentLocation> departmentLocations,
        Department? parent = null,
        Guid? departmentId = null)
    {
        var deParent = parent is null
            ? CreateParent(name, slug, departmentLocations, departmentId)
            : CreateChild(name, slug, parent, departmentLocations, departmentId);
        return deParent;
    }

    private static Result<Department, Errors> CreateChild(
        string name, Slug slug, Department parent,
        IReadOnlyCollection<DepartmentLocation> departmentLocations,
        Guid? id = null
    )
    {
        var locationsValidationResult = ValidateDepartment(name, departmentLocations);
        if (locationsValidationResult.IsFailure)
            return Result.Failure<Department, Errors>(locationsValidationResult.Error);

        var path = Path.CreateForChild(parent.Path, slug);
        if (path.IsFailure)
            return Result.Failure<Department, Errors>(path.Error);

        return new Department(
            id ?? Guid.NewGuid(),
            name,
            slug,
            path.Value,
            parent.Depth + 1,
            parent.Id,
            departmentLocations);
    }

    private static Result<Department, Errors> CreateParent(
        string name, Slug slug,
        IReadOnlyCollection<DepartmentLocation> departmentLocations,
        Guid? id = null)
    {
        var locationsValidationResult = ValidateDepartment(name, departmentLocations);
        if (locationsValidationResult.IsFailure)
            return Result.Failure<Department, Errors>(locationsValidationResult.Error);

        var path = Path.CreateForParent(slug.Value);
        if (path.IsFailure)
            return Result.Failure<Department, Errors>(path.Error);

        return new Department(
            id ?? Guid.NewGuid(),
            name,
            slug,
            path.Value,
            0,
            null,
            departmentLocations);
    }

    private static UnitResult<Errors> ValidateDepartment(
        string name,
        IReadOnlyCollection<DepartmentLocation> departmentLocations)
    {
        var nameValidationResult = Guard.ValidateStringField(name, nameof(Name), 3, 150);
        if (nameValidationResult.IsFailure)
            return UnitResult.Failure(nameValidationResult.Error);

        if (departmentLocations.Count == 0)
        {
            return Error.Validation(
                "department.location", "Department locations must contain at least one location",
                nameof(Department.DepartmentLocations)).ToErrors();
        }

        return UnitResult.Success<Errors>();
    }

    private static UnitResult<Errors> ValidateDepth(int depth, Guid? parentId)
    {
        const string fieldName = nameof(Depth);
        if (depth < 0)
        {
            return UnitResult.Failure<Errors>(
                GeneralErrors.ValueIsInvalid(
                    fieldName, $"{fieldName} must be greater than or equal to zero"));
        }

        if (depth > 0 && !parentId.HasValue)
        {
            return UnitResult.Failure<Errors>(
                GeneralErrors.ValueIsInvalid(
                    fieldName, "Department with non-zero depth must have a parent"));
        }

        if (depth == 0 && parentId.HasValue)
        {
            return UnitResult.Failure<Errors>(
                GeneralErrors.ValueIsInvalid(
                    fieldName, "Department with zero depth cannot have a parent"));
        }

        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> Rename(string newName)
    {
        if (string.Equals(Name, newName, StringComparison.Ordinal))
            return UnitResult.Success<Errors>();

        var nameValidation = Guard.ValidateStringField(newName, nameof(Name), 3, 150);
        if (nameValidation.IsFailure)
        {
            return UnitResult.Failure(nameValidation.Error);
        }

        Name = newName;
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> UpdateSlug(Slug slug)
    {
        Slug = slug;
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> MoveTo(Guid? newParentId, Path newPath, int newDepth)
    {
        var validationResult = ValidateDepth(newDepth, newParentId);
        if (validationResult.IsFailure)
        {
            return UnitResult.Failure(validationResult.Error);
        }

        ParentId = newParentId;
        Path = newPath;
        Depth = newDepth;
        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> AddLocation(Guid locationId)
    {
        if (_departmentLocations.All(dl => dl.LocationId != locationId))
        {
            _departmentLocations.Add(new DepartmentLocation(Id, locationId));
            return UnitResult.Success<Errors>();
        }

        return GeneralErrors.AlreadyExists(
                locationId, $"The location '{locationId}' has already been added to the department.")
            .ToErrors();
    }

    public UnitResult<Errors> RemoveLocation(Guid locationId)
    {
        if (_departmentLocations.Count == 1 && _departmentLocations[0].LocationId == locationId)
        {
            return Error.Conflict(
                    code: "delete.department.location.last.relation",
                    message: $"Cannot remove the last location {locationId} from department {Id}. " +
                             "Department must have at least one location.")
                .ToErrors();
        }

        var removed = _departmentLocations.RemoveAll(dl => dl.LocationId == locationId);
        if (removed <= 0)
        {
            return GeneralErrors
                .ValueIsInvalid($"There are no locations in the department with the given id: {locationId}")
                .ToErrors();
        }

        return UnitResult.Success<Errors>();
    }

    public UnitResult<Errors> AddPosition(Guid positionId)
    {
        if (_departmentPositions.All(dp => dp.PositionId != positionId))
        {
            _departmentPositions.Add(new DepartmentPosition(Id, positionId));
            return UnitResult.Success<Errors>();
        }

        return GeneralErrors.AlreadyExists(
                positionId, "The position '" + positionId + "' has already been added to the department.")
            .ToErrors();
    }

    public UnitResult<Errors> RemovePosition(Guid positionId)
    {
        var removed = _departmentPositions.RemoveAll(dp => dp.PositionId == positionId);
        if (removed <= 0)
        {
            return GeneralErrors.NotFound(
                    id: positionId,
                    message: $"There are no positions in the department with the given id: {positionId}")
                .ToErrors();
        }

        return UnitResult.Success<Errors>();
    }
}