using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public sealed class DepartmentPositionConfiguration : BaseEntityConfiguration<DepartmentPosition>
{
    public override void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        base.Configure(builder);

        builder
            .Property(dp => dp.PositionId)
            .HasColumnName("position_id");

        builder
            .Property(dp => dp.DepartmentId)
            .HasColumnName("department_id");

        builder
            .HasIndex(dp => new { dp.DepartmentId, dp.PositionId })
            .IsUnique();

        builder
            .HasOne<Department>()
            .WithMany(d => d.DepartmentPositions)
            .HasForeignKey(dp => dp.DepartmentId)
            .HasConstraintName($"fk_{TableName}_department_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Position>()
            .WithMany(p => p.DepartmentPositions)
            .HasForeignKey(dp => dp.PositionId)
            .HasConstraintName($"fk_{TableName}_position_id")
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override string TableName => "department_positions";
}