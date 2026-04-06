using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.Postgres.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres;

public class DirectoryServiceDbContext : DbContext
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

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Location> Locations => Set<Location>();

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
}