using CSharpFunctionalExtensions;
using DirectoryService.Domain.Common;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Domain.Entities;

public sealed class Location : AuditableEntity
{
    private const int NameMinLength = 3;

    private const int NameMaxLength = 120;

    private List<Department> _departments = [];

    public string Name { get; private set; }

    public Address? Address { get; private set; }

    public Timezone Timezone { get; private set; }

    public IReadOnlyList<Department> Departments => _departments;

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

    internal void AddDepartment(Department department)
    {
        if (_departments.All(d => d.Id != department.Id))
            _departments.Add(department);
    }

    internal void RemoveDepartment(Guid departmentId)
    {
        _departments.RemoveAll(d => d.Id == departmentId);
    }
}