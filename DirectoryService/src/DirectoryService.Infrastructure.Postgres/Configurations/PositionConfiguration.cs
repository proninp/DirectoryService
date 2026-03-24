using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public sealed class PositionConfiguration : BaseEntityConfiguration<Position>
{
    public override void Configure(EntityTypeBuilder<Position> builder)
    {
        base.Configure(builder);

        builder
            .Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(Position.NameMaxLength)
            .HasColumnName("name");

        builder
            .HasIndex(e => e.Name)
            .IsUnique();

        builder
            .Property(e => e.Description)
            .HasMaxLength(Position.DescriptionMaxLength)
            .IsRequired(false)
            .HasColumnName("description");

        builder
            .Navigation(p => p.DepartmentPositions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    protected override string TableName => "positions";
}