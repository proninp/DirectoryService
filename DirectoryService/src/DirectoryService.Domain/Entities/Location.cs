using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Domain.Entities;

public sealed class Location : BaseEntity
{
    private const int NameMinLength = 3;

    private const int NameMaxLength = 120;

    private List<DepartmentLocation> _departmentLocations = [];

    public string Name { get; private set; }

    public Address? Address { get; private set; }

    public Timezone Timezone { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    private Location(string name, Address? address, Timezone timezone)
        : base(Guid.NewGuid())
    {
        Name = name;
        Address = address;
        Timezone = timezone;
    }

    public static Result<Location> Create(string name, Address? address, Timezone timezone)
    {
        var validationResult = Guard.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Location>(validationResult.Error);
        }

        return new Location(name, address, timezone);
    }

    public Result Rename(string name)
    {
        var validation = Guard.ValidateStringField(name, nameof(Name), NameMinLength, NameMaxLength);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Error);
        }

        Name = name;
        return Result.Success();
    }

    public void UpdateAddress(Address? address) => Address = address;

    public void UpdateTimezone(Timezone timezone) => Timezone = timezone;

    internal Result AddDepartment(Guid departmentId)
    {
        if (_departmentLocations.All(dl => dl.DepartmentId != departmentId))
        {
            _departmentLocations.Add(new DepartmentLocation(Id, departmentId));
            return Result.Success();
        }

        return Result.Failure("The department '" + departmentId + "' has already been added to the location.");
    }

    internal Result RemoveDepartment(Guid departmentId)
    {
        var removed = _departmentLocations.RemoveAll(dl => dl.DepartmentId == departmentId);
        return removed > 0
            ? Result.Success()
            : Result.Failure("There are no departments for the location with the given id: " + departmentId);
    }
}