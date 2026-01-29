using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.Posts;
using Lanka.Modules.Analytics.Domain.UserActivities;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;
using MongoDB.Driver;
using Serilog;

namespace Lanka.Modules.Analytics.Infrastructure.Database.Seeders;

public static class AnalyticsDataSeeder
{
    public static async Task SeedAsync(
        IMongoClient mongoClient,
        IReadOnlyList<Guid> instagramAccountIds,
        CancellationToken ct = default
    )
    {
        IMongoDatabase db = mongoClient.GetDatabase(DocumentDbSettings.Database);

        IMongoCollection<InstagramPosts> postsCollection =
            db.GetCollection<InstagramPosts>(DocumentDbSettings.InstagramPosts);

        // Get accounts that already have data
        List<Guid>? existingAccountIds = await postsCollection
            .Find(Builders<InstagramPosts>.Filter.In(p => p.InstagramAccountId, instagramAccountIds))
            .Project(p => p.InstagramAccountId)
            .ToListAsync(ct);

        var accountsToSeed = instagramAccountIds.Except(existingAccountIds).ToList();

        if (accountsToSeed.Count == 0)
        {
            Log.Information("MongoDB analytics data already exists for all {Count} accounts", instagramAccountIds.Count);
            return;
        }

        Log.Information("Seeding MongoDB analytics for {Count} accounts (out of {Total} total)",
            accountsToSeed.Count, instagramAccountIds.Count);

        foreach (Guid accountId in accountsToSeed)
        {
            await SeedPostsForAccount(db, accountId, ct);
            await SeedAudienceDataForAccount(db, accountId, ct);
        }

        Log.Information("Seeded MongoDB analytics for {Count} new accounts", accountsToSeed.Count);
    }

    private static async Task SeedPostsForAccount(
        IMongoDatabase db,
        Guid accountId,
        CancellationToken ct
    )
    {
        IMongoCollection<InstagramPosts> collection =
            db.GetCollection<InstagramPosts>(DocumentDbSettings.InstagramPosts);

        // Check if posts already exist for this account
        long existingCount = await collection.CountDocumentsAsync(
            Builders<InstagramPosts>.Filter.Eq(p => p.InstagramAccountId, accountId),
            cancellationToken: ct);

        if (existingCount > 0)
        {
            return;
        }

        var posts = new InstagramPosts(UserActivityLevel.Active)
        {
            InstagramAccountId = accountId,
            Posts = FakeInstagramDataGenerator.GeneratePosts(6),
            Paging = FakeInstagramDataGenerator.GeneratePagingInfo()
        };

        await collection.InsertOneAsync(posts, cancellationToken: ct);
    }

    private static async Task SeedAudienceDataForAccount(
        IMongoDatabase db,
        Guid accountId,
        CancellationToken ct)
    {
        // Seed age distribution
        IMongoCollection<AgeDistribution> ageCollection =
            db.GetCollection<AgeDistribution>(DocumentDbSettings.AgeDistribution);
        long ageCount = await ageCollection.CountDocumentsAsync(
            Builders<AgeDistribution>.Filter.Eq(a => a.InstagramAccountId, accountId),
            cancellationToken: ct);

        if (ageCount == 0)
        {
            await ageCollection.InsertOneAsync(
                FakeInstagramDataGenerator.GenerateAgeDistributionDomain(accountId),
                cancellationToken: ct);
        }

        // Seed gender distribution
        IMongoCollection<GenderDistribution> genderCollection =
            db.GetCollection<GenderDistribution>(DocumentDbSettings.GenderDistribution);
        long genderCount = await genderCollection.CountDocumentsAsync(
            Builders<GenderDistribution>.Filter.Eq(g => g.InstagramAccountId, accountId),
            cancellationToken: ct);

        if (genderCount == 0)
        {
            await genderCollection.InsertOneAsync(
                FakeInstagramDataGenerator.GenerateGenderDistributionDomain(accountId),
                cancellationToken: ct);
        }

        // Seed location distribution
        IMongoCollection<LocationDistribution> locationCollection =
            db.GetCollection<LocationDistribution>(DocumentDbSettings.LocationDistribution);
       
        long locationCount = await locationCollection.CountDocumentsAsync(
            Builders<LocationDistribution>.Filter.Eq(l => l.InstagramAccountId, accountId),
            cancellationToken: ct);

        if (locationCount == 0)
        {
            await locationCollection.InsertOneAsync(
                FakeInstagramDataGenerator.GenerateLocationDistributionDomain(accountId, LocationType.City),
                cancellationToken: ct);
        }
    }
}
