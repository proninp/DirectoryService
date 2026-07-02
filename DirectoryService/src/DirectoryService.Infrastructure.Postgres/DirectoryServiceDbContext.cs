using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.Abstractions;
using DirectoryService.Infrastructure.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres;

public class DirectoryServiceDbContext : DbContext, IReadDbContext
{
    private readonly DbSettings _dbSettings;
    private readonly IHostEnvironment _environment;
    private readonly ILoggerFactory _loggerFactory;

    public DirectoryServiceDbContext(
        DbContextOptions<DirectoryServiceDbContext> options,
        IHostEnvironment environment,
        IOptions<DbSettings> dbSettingsOptions,
        ILoggerFactory loggerFactory
    )
        : base(options)
    {
        _dbSettings = dbSettingsOptions.Value;
        _environment = environment;
        _loggerFactory = loggerFactory;
    }

    public DbSet<Department> Departments => Set<Department>();

    public IQueryable<Department> DepartmentsQuery => Departments.AsQueryable().AsNoTracking();

    public DbSet<Position> Positions => Set<Position>();

    public IQueryable<Position> PositionsQuery => Positions.AsQueryable().AsNoTracking();

    public DbSet<Location> Locations => Set<Location>();

    public IQueryable<Location> LocationsQuery => Locations.AsQueryable().AsNoTracking();

    public DbSet<DepartmentPosition> DepartmentPositions => Set<DepartmentPosition>();

    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_dbSettings.ConnectionString);
        if (_environment.IsDevelopment())
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseLoggerFactory(_loggerFactory);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity.DeletedAt == null)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}