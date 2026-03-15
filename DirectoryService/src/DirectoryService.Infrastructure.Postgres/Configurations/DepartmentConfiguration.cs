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
            .ToTable(TableName);

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
            .HasConstraintName("fk_department_parent");

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
    }

    protected override string TableName => "departments";
}