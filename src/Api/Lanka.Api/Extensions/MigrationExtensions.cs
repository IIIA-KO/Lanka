using Lanka.Modules.Analytics.Infrastructure.Database;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Lanka.Modules.Matching.Infrastructure.Database;
using Lanka.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Api.Extensions;

internal static class MigrationExtensions
{
    extension(IApplicationBuilder app)
    {
        internal void ApplyMigrations()
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();

            ApplyMigration<UsersDbContext>(serviceScope);
            ApplyMigration<CampaignsDbContext>(serviceScope);
            ApplyMigration<AnalyticsDbContext>(serviceScope);
            ApplyMigration<MatchingDbContext>(serviceScope);
        }
    }

    private static void ApplyMigration<TDbContext>(IServiceScope serviceScope)
        where TDbContext : DbContext
    {
        using TDbContext dbContext =
            serviceScope.ServiceProvider.GetRequiredService<TDbContext>();

        dbContext.Database.Migrate();
    }
}
