using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public sealed class DepartmentConfiguration : BaseEntityConfiguration<Department>
{
    public override void Configure(EntityTypeBuilder<Department> builder)
    {
        base.Configure(builder);

        builder
            .Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("name");

        builder
            .ComplexProperty(d => d.Identifier, pb =>
                {
                    pb.Property(i => i.Value)
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnName("identifier");
                }
            );

        builder
            .HasOne(d => d.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(d => d.ParentId)
            .HasConstraintName("fk_department_parent")
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .IsRequired(false);

        builder
            .ComplexProperty(d => d.Path, pb =>
                {
                    pb.Property(p => p.Value)
                        .IsRequired(false)
                        .HasColumnName("path");
                }
            );

        builder
            .Property(d => d.Depth)
            .HasColumnName("depth");

        builder
            .Navigation(d => d.Children)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .Navigation(d => d.DepartmentLocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .Navigation(d => d.DepartmentPositions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    protected override string TableName => "departments";
}