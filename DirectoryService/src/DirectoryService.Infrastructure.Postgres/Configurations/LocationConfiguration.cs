using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public sealed class LocationConfiguration : BaseEntityConfiguration<Location>
{
    public override void Configure(EntityTypeBuilder<Location> builder)
    {
        base.Configure(builder);

        builder
            .Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(120)
            .HasColumnName("name");

        builder
            .ComplexProperty(l => l.Address, pb =>
                {
                    pb.IsRequired(false);

                    pb
                        .Property(e => e.PostalCode)
                        .HasColumnName("postal_code")
                        .HasMaxLength(Address.PostalCodeMaxLength)
                        .IsRequired();

                    pb
                        .Property(e => e.Country)
                        .HasColumnName("country")
                        .HasMaxLength(Address.CountryMaxLength)
                        .IsRequired();

                    pb
                        .Property(e => e.City)
                        .HasColumnName("city")
                        .HasMaxLength(Address.CityMaxLength)
                        .IsRequired();

                    pb
                        .Property(e => e.Street)
                        .HasColumnName("street")
                        .HasMaxLength(Address.StreetMaxLength)
                        .IsRequired();

                    pb
                        .Property(e => e.House)
                        .HasColumnName("house")
                        .HasMaxLength(Address.HouseMaxLength)
                        .IsRequired();

                    pb
                        .Property(e => e.Block)
                        .HasColumnName("block")
                        .HasMaxLength(Address.BlockMaxLength)
                        .IsRequired(false);

                    pb
                        .Property(e => e.Room)
                        .HasColumnName("room")
                        .HasMaxLength(Address.RoomMaxLength)
                        .IsRequired(false);

                    pb
                        .Property(e => e.PostalBox)
                        .HasColumnName("postal_box")
                        .HasMaxLength(Address.PostalBoxMaxLength)
                        .IsRequired(false);
                }
            );

        builder.ComplexProperty(l => l.Timezone, pb =>
            {
                pb.Property(e => e.Value)
                    .HasColumnName("timezone")
                    .HasMaxLength(50)
                    .IsRequired();
            }
        );

        builder
            .Navigation(l => l.DepartmentLocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    protected override string TableName => "locations";
}