using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectorySevice.Infrastructure.Postgres.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder
            .HasKey(d => d.Id)
            .HasName("pk_department");

        builder
            .Property(d => d.Id)
            .HasColumnName("id");

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
            .ComplexProperty(d => d.Path, pb =>
                {
                    pb.Property(p => p.Value)
                        .IsRequired(false)
                        .HasColumnName("path");
                }
            );
    }
}