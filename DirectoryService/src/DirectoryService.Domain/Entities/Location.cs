using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Entities;

public sealed class Location : BaseEntity
{
    private const int NameMinLength = 3;

    private const int NameMaxLength = 120;

    private readonly List<DepartmentLocation> _departmentLocations = [];

    public string Name { get; private set; } = null!;

    public Address? Address { get; private set; }

    public Timezone Timezone { get; private set; } = null!;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    // EF Core Constructor
    private Location() { }

    private Location(string name, Address? address, Timezone timezone)
        : base(Guid.NewGuid())
    {
        Name = name;
        Address = address;
        Timezone = timezone;
    }

    public static Result<Location, Error> Create(string name, Address? address, Timezone timezone)
    {
        var validationResult = Guard.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength);

        if (validationResult.IsFailure)
            return Result.Failure<Location, Error>(validationResult.Error);

        return new Location(name, address, timezone);
    }

    public UnitResult<Error> Rename(string name)
    {
        var validation = Guard.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength);
        if (validation.IsFailure)
        {
            return UnitResult.Failure(validation.Error);
        }

        Name = name;
        return UnitResult.Success<Error>();
    }

    public void UpdateAddress(Address? address) => Address = address;

    public void UpdateTimezone(Timezone timezone) => Timezone = timezone;

    internal UnitResult<Error> AddDepartment(Guid departmentId)
    {
        if (_departmentLocations.All(dl => dl.DepartmentId != departmentId))
        {
            _departmentLocations.Add(new DepartmentLocation(Id, departmentId));
            return UnitResult.Success<Error>();
        }

        return GeneralError.AlreadyExists(
            departmentId, $"The department '{departmentId}' has already been added to the location.");
    }

    internal UnitResult<Error> RemoveDepartment(Guid departmentId)
    {
        var removed = _departmentLocations.RemoveAll(dl => dl.DepartmentId == departmentId);
        return removed > 0
            ? UnitResult.Success<Error>()
            : GeneralError.NotFound(departmentId, nameof(Department),
                $"There are no departments for the location with the given id: {departmentId}");
    }
}