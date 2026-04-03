using DirectoryService.Domain.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder
            .ToTable(TableName);

        builder.HasKey(d => d.Id)
            .HasName($"pk_{TableName}");

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())")
            .HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at");
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.HasQueryFilter(d => d.IsActive);
    }

    protected abstract string TableName { get; }
}