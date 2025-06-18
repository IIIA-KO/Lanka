using Lanka.Modules.Analytics.Infrastructure;
using Lanka.Modules.Analytics.Infrastructure.Database;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Lanka.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Api.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope serviceScope = app.ApplicationServices.CreateScope();

        ApplyMigration<UsersDbContext>(serviceScope);
        ApplyMigration<CampaignsDbContext>(serviceScope);
        ApplyMigration<AnalyticsDbContext>(serviceScope);
    }

    private static void ApplyMigration<TDbContext>(IServiceScope serviceScope)
        where TDbContext : DbContext
    {
        using TDbContext dbContext =
            serviceScope.ServiceProvider.GetRequiredService<TDbContext>();

        dbContext.Database.Migrate();
    }
}
