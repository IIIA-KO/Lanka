using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using EFCore.NamingConventions;

namespace Lanka.Modules.Users.Infrastructure.Database;

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
public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    /// <summary>
    /// Creates a new instance of UsersDbContext configured for design-time operations.
    /// This method is called by EF Core tooling and should not be used in application code.
    /// </summary>
    /// <param name="args">Command-line arguments passed from EF Core tools (unused)</param>
    /// <returns>Configured UsersDbContext instance for migrations</returns>
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        
        // Get connection string from environment or use local development default
        string connectionString = GetConnectionString();
        
        // Configure PostgreSQL with module-specific schema and naming conventions
        // Note: Runtime interceptors (like InsertOutboxMessagesInterceptor) are intentionally excluded
        // as they are not needed for schema migrations and would cause dependency issues
        optionsBuilder
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
            .UseSnakeCaseNamingConvention();

        return new UsersDbContext(optionsBuilder.Options);
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