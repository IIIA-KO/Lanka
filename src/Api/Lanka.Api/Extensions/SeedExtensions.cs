using System.Data.Common;
using Lanka.Common.Application.Data;
using Lanka.Common.Contracts.Seeding;
using Lanka.Modules.Analytics.Infrastructure.Database.Seeders;
using Lanka.Modules.Analytics.Infrastructure.Encryption;
using Lanka.Modules.Campaigns.Infrastructure.Database.Seeders;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Seeders;
using Lanka.Modules.Users.Infrastructure.Database.Seeders;
using MongoDB.Driver;
using Serilog;

namespace Lanka.Api.Extensions;

internal static class SeedExtensions
{
    internal static async Task SeedDevelopmentDataAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        IWebHostEnvironment environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (!environment.IsDevelopment())
        {
            return;
        }

        IDbConnectionFactory dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        IConfiguration config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var seedingOptions = new SeedingOptions();
        config.GetSection("Development:Seeding").Bind(seedingOptions);

        if (!seedingOptions.Enabled)
        {
            Log.Information("Database seeding is disabled");
            return;
        }

        Log.Information("Starting database seeding ({FakeUserCount} fake users)", seedingOptions.FakeUserCount);

        try
        {
            IMongoClient mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();

            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            await using DbTransaction transaction = await connection.BeginTransactionAsync();

            // Seed Users Module
            Log.Information("Seeding Users module");
            List<UserSeedData> users = await UserSeeder.SeedAsync(connection, transaction, seedingOptions.FakeUserCount);

            if (users.Count == 0)
            {
                Log.Warning("No users created, aborting seeding");
                await transaction.RollbackAsync();
                return;
            }

            var userIds = users.Select(u => u.Id).ToList();

            // Seed Analytics Module
            Log.Information("Seeding Analytics module");
            EncryptionService encryptionService = scope.ServiceProvider.GetRequiredService<EncryptionService>();

            (List<Guid> igAccountIds, List<InstagramAccountSeedData> igAccountData) =
                await InstagramAccountSeeder.SeedAsync(connection, transaction, users, encryptionService);
            await AnalyticsDataSeeder.SeedAsync(mongoClient, igAccountIds);

            // Seed Campaigns Module
            Log.Information("Seeding Campaigns module");

            // All users become bloggers (must be done AFTER Instagram accounts to populate metadata)
            (List<Guid> bloggerIds, List<BloggerSeedData> bloggerData) =
                await BloggerSeeder.SeedAsync(connection, transaction, userIds, igAccountData);
            Log.Information("Created {BloggerCount} bloggers from {UserCount} users", bloggerIds.Count, userIds.Count);

            // Create pacts for all bloggers (each blogger has one pact with their service offerings)
            (List<Guid> pactIds, List<PactSeedData> pactData) =
                await PactSeeder.SeedAsync(connection, transaction, bloggerIds);
            Log.Information("Created {PactCount} pacts", pactIds.Count);

            // Create offers in pacts (each pact has 3-5 service offers)
            (List<Guid> offerIds, List<OfferSeedData> offerData) =
                await OfferSeeder.SeedAsync(connection, transaction, pactIds);
            Log.Information("Created {OfferCount} offers across all pacts", offerIds.Count);

            // Create campaigns between bloggers
            (List<Guid> campaignIds, List<CampaignSeedData> campaignData) =
                await CampaignSeeder.SeedAsync(
                    connection,
                    transaction,
                    bloggerIds,
                    offerIds,
                    seedingOptions.FakeCampaignsPerBlogger);
            Log.Information("Created {CampaignCount} campaigns", campaignIds.Count);

            // Create reviews for completed campaigns
            (List<Guid> reviewIds, List<ReviewSeedData> reviewData) =
                await ReviewSeeder.SeedAsync(connection, transaction, campaignIds);
            Log.Information("Total {ReviewCount} reviews", reviewIds.Count);

            // Commit all database changes
            await transaction.CommitAsync();

            Log.Information("Database seeding completed successfully");
            Log.Information(
                "Summary: {UserCount} users, {AccountCount} Instagram accounts, {BloggerCount} bloggers, {PactCount} pacts, {OfferCount} offers, {CampaignCount} campaigns, {ReviewCount} reviews",
                userIds.Count,
                igAccountIds.Count,
                bloggerIds.Count,
                pactIds.Count,
                offerIds.Count,
                campaignIds.Count,
                reviewIds.Count);

            // Seed Elasticsearch
            try
            {
                ISearchIndexService? searchIndexService = scope.ServiceProvider.GetService<ISearchIndexService>();

                if (searchIndexService is null)
                {
                    Log.Warning("Elasticsearch search index service not available, skipping search seeding");
                }
                else
                {
                    Log.Information("Seeding Elasticsearch search index");

                    await ElasticsearchSeeder.SeedAsync(
                        searchIndexService,
                        bloggerData,
                        igAccountData,
                        pactData,
                        offerData,
                        campaignData,
                        reviewData
                    );

                    Log.Information("Elasticsearch seeding completed successfully");
                }
            }
            catch (Exception esEx)
            {
                Log.Error(esEx,
                    "Failed to seed Elasticsearch, but database seeding was successful. Search functionality may be limited until data is synced.");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database seeding failed - application startup aborted");
        }
    }
}
