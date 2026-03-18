using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public sealed class DepartmentLocationConfiguration : BaseEntityConfiguration<DepartmentLocation>
{
    public override void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        base.Configure(builder);

        builder
            .Property(dl => dl.LocationId)
            .HasColumnName("location_id");

        builder
            .Property(dl => dl.DepartmentId)
            .HasColumnName("department_id");

        builder
            .HasIndex(dl => new { dl.DepartmentId, dl.LocationId })
            .IsUnique();

        builder
            .HasOne<Department>()
            .WithMany(d => d.DepartmentLocations)
            .HasForeignKey(dl => dl.DepartmentId)
            .HasConstraintName($"fk_{TableName}_department_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Location>()
            .WithMany(l => l.DepartmentLocations)
            .HasForeignKey(dl => dl.LocationId)
            .HasConstraintName($"fk_{TableName}_location_id")
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override string TableName => "department_locations";
}