using DirectoryService.Domain.Entities.ValueObjects;
using SharedKernel.Domain.Entities;
using SharedKernel.Domain.IDs;

namespace DirectoryService.Domain.Entities;

public sealed class Location : AuditableEntity<LocationId>
{
    public string Name { get; set; }

    public Address? Address { get; set; }

    public Timezone Timezone { get; set; }

    public Location(string name, Address? address, Timezone timezone)
        : base(LocationId.Create())
    {
        Name = name;
        Address = address;
        Timezone = timezone;
    }
}