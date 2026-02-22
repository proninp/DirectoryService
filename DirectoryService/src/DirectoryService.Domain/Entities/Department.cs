using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using SharedKernel.Domain.Entities;
using SharedKernel.Domain.IDs;

namespace DirectoryService.Domain.Entities;

public sealed class Department : AuditableEntity<DepartmentId>
{
    private List<Department> _children = [];

    public string Name { get; private set; }

    public string Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public Department? Parent { get; private set; }

    public IReadOnlyList<Department> Children => _children;

    public string Path { get; private set; }

    public int Depth { get; private set; }

    public IReadOnlyList<Location> Locations { get; private set; }

    private Department(string name, string identifier, Guid? parentId, string path, int depth,
        IEnumerable<Location> locations
    )
        : base(DepartmentId.Create())
    {
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        Locations = locations.ToList();
    }

    public static Result<Department> Create(string name, string identifier, Guid? parentId, string path, int depth,
        IEnumerable<Location> locations
    )
    {
        var locationsList = locations as ICollection<Location> ?? [.. locations];

        var validationResult = Result.Combine(
            Guard.ValidateStringField(name, nameof(Name), 3, 150),
            Guard.ValidateStringField(identifier, nameof(Identifier), 3, 150)
                .Bind(() => Guard.ValidateLatinString(identifier, nameof(Identifier))),
            locationsList.Count == 0
                ? Result.Failure<Department>($"No locations found for {name}")
                : Result.Success()
        );

        if (validationResult.IsFailure)
            return Result.Failure<Department>(validationResult.Error);

        var department = new Department(name, identifier, parentId, path, depth, locationsList);
        return Result.Success(department);
    }
}