using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lanka.Modules.Matching.Infrastructure.Database;

/// <summary>
/// Design-time factory for creating UsersDbContext instances during EF Core migrations.
/// This factory is used exclusively by EF Core tooling (dotnet ef migrations) and is not used at runtime.
/// 
/// Usage:
/// - dotnet ef migrations add MigrationName --project src/Modules/Users/Lanka.Modules.Users.Infrastructure
/// - dotnet ef database update --project src/Modules/Users/Lanka.Modules.Users.Infrastructure
/// 
/// Configuration:
/// - Set DESIGN_TIME_CONNECTION_STRING environment variable to override default connection string
/// - Default connection string assumes local PostgreSQL development setup
/// </summary>
public class MatchingDbContextFactory : IDesignTimeDbContextFactory<MatchingDbContext>
{
    public MatchingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MatchingDbContext>();

        // Get connection string from environment or use local development default
        string connectionString = GetConnectionString();

        // Configure PostgreSQL with module-specific schema and naming conventions
        // Note: Runtime interceptors (like InsertOutboxMessagesInterceptor) are intentionally excluded
        // as they are not needed for schema migrations and would cause dependency issues
        optionsBuilder
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Matching))
            .UseSnakeCaseNamingConvention();

        return new MatchingDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the connection string for design-time operations.
    /// Checks environment variable first, falls back to local development default.
    /// </summary>
    /// <returns>PostgreSQL connection string</returns>
    private static string GetConnectionString()
    {
        // Check for environment variable first (useful for CI/CD or different dev environments)
        string? envConnectionString = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING");

        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        // Default connection string for local development
        // Assumes standard PostgreSQL setup with database 'lanka_dev'
        return "Host=localhost;Database=lanka_dev;Username=postgres;Password=postgres";
    }
}
