using System.Data;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Infrastructure.Postgres.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres;

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _dbConnectionString;

    public DbConnectionFactory(IOptions<DbSettings> dbSettingsOptions)
    {
        _dbConnectionString = dbSettingsOptions.Value.ConnectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}