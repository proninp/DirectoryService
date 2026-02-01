using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Domain.Entities.ValueObjects;

namespace DirectoryService.Domain.Entities;

public sealed class Location : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public Address? Address { get; set; }

    public Timezone Timezone { get; set; }

    public Location(string name, Address? address, Timezone timezone)
    {
        Name = name;
        Address = address;
        Timezone = timezone;
    }
}