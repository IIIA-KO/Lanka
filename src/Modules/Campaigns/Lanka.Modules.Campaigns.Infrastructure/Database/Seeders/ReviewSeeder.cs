using System.Data;
using Bogus;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Serilog;

namespace Lanka.Modules.Campaigns.Infrastructure.Database.Seeders;

public static class ReviewSeeder
{
    private static readonly int[] _ratingValues = [1, 2, 3, 4, 5];
    private static readonly float[] _ratingWeights = [0.05f, 0.05f, 0.15f, 0.35f, 0.40f];

    private static readonly string[] _fiveStarComments =
    [
        "Excellent work! Highly professional and delivered beyond expectations.",
        "Outstanding collaboration! Would definitely work together again.",
        "Perfect execution! Very satisfied with the results.",
        "Amazing experience! Communication was great throughout.",
        "Superb quality! Exceeded all expectations."
    ];

    private static readonly string[] _fourStarComments =
    [
        "Great work! Very professional and timely delivery.",
        "Good collaboration. Minor adjustments needed but overall satisfied.",
        "Solid performance. Would recommend.",
        "Very good work. Met all the requirements.",
        "Pleasant experience. Good communication."
    ];

    private static readonly string[] _threeStarComments =
    [
        "Decent work. Some room for improvement.",
        "Acceptable results. Could be better with more attention to detail.",
        "Average performance. Met basic expectations.",
        "OK collaboration. Had some communication issues.",
        "Fair work. Needed several revisions."
    ];

    private static readonly string[] _twoStarComments =
    [
        "Below expectations. Had to provide extensive feedback.",
        "Disappointing. Multiple revisions required.",
        "Poor communication. Delays in delivery.",
        "Subpar quality. Not what was agreed upon.",
        "Unsatisfactory. Would not recommend."
    ];

    private static readonly string[] _oneStarComments =
    [
        "Very disappointing. Did not meet requirements at all.",
        "Poor experience. Had to cancel midway.",
        "Unprofessional behavior. Would not work together again.",
        "Terrible communication. Missed all deadlines.",
        "Completely unsatisfactory. Waste of time and money."
    ];

    public static async Task<(List<Guid> Ids, List<ReviewSeedData> Data)> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> campaignIds
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(campaignIds);

        if (!campaignIds.Any())
        {
            Log.Warning("No campaign IDs provided for review seeding");
            return ([], []);
        }

        // Get completed campaigns from the provided campaign IDs
        List<(Guid CampaignId, Guid CreatorId, Guid ClientId, Guid OfferId, DateTimeOffset CompletedOnUtc, string
            CampaignName)> completedCampaigns
            = await GetCompletedCampaignsAsync(
                connection,
                transaction,
                campaignIds
            );

        if (completedCampaigns.Count == 0)
        {
            Log.Warning("No completed campaigns found, skipping review seeding");
            return ([], []);
        }

        // Check which campaigns already have reviews
        var completedCampaignIds = completedCampaigns.Select(c => c.CampaignId).ToList();
        List<Guid> campaignsWithReviews = await GetCampaignsWithReviewsAsync(connection, transaction, completedCampaignIds);
        List<Guid> existingReviewIds = await GetExistingReviewIdsForCampaignsAsync(connection, transaction, completedCampaignIds);

        if (campaignsWithReviews.Count == completedCampaigns.Count)
        {
            Log.Information("Reviews already exist for all {Count} completed campaigns", completedCampaigns.Count);
            List<ReviewSeedData> existingData = await GetExistingReviewDataAsync(connection, transaction, existingReviewIds);
            return (existingReviewIds, existingData);
        }

        // Filter to campaigns that don't have reviews yet
        var campaignsWithReviewsSet = new HashSet<Guid>(campaignsWithReviews);
        var campaignsWithoutReviews = completedCampaigns
            .Where(c => !campaignsWithReviewsSet.Contains(c.CampaignId))
            .ToList();

        if (campaignsWithoutReviews.Count == 0)
        {
            Log.Information("All completed campaigns already have reviews");
            List<ReviewSeedData> existingData = await GetExistingReviewDataAsync(connection, transaction, existingReviewIds);
            return (existingReviewIds, existingData);
        }

        Log.Information("Creating reviews for {Count} campaigns (out of {Total} completed)",
            campaignsWithoutReviews.Count, completedCampaigns.Count);

        List<ReviewSeedData> reviews = GenerateSeedData(campaignsWithoutReviews);

        await InsertReviewsAsync(connection, transaction, reviews);

        var newReviewIds = reviews.Select(r => r.Id).ToList();

        Log.Information("Seeded {Count} reviews for {CampaignCount} completed campaigns", reviews.Count,
            campaignsWithoutReviews.Count);

        // Return all review IDs (existing + newly created)
        var allReviewIds = existingReviewIds.Concat(newReviewIds).ToList();
        List<ReviewSeedData> allData = await GetExistingReviewDataAsync(connection, transaction, allReviewIds);

        return (allReviewIds, allData);
    }

    private static async Task<List<Guid>> GetCampaignsWithReviewsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> campaignIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                          SELECT DISTINCT campaign_id
                          FROM campaigns.reviews
                          WHERE campaign_id = ANY(@CampaignIds)
                          """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { CampaignIds = campaignIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> campaignIdsWithReviews = await connection.QueryAsync<Guid>(commandDefinition);
        return campaignIdsWithReviews.AsList();
    }

    private static async Task<List<Guid>> GetExistingReviewIdsForCampaignsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> campaignIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT id
                           FROM campaigns.reviews
                           WHERE campaign_id = ANY(@CampaignIds)
                           ORDER BY created_on_utc
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { CampaignIds = campaignIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> ids = await connection.QueryAsync<Guid>(commandDefinition);
        return ids.AsList();
    }

    private static async Task<List<ReviewSeedData>> GetExistingReviewDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> reviewIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT
                               r.id AS Id,
                               r.campaign_id AS CampaignId,
                               r.client_id AS ClientId,
                               r.creator_id AS CreatorId,
                               r.offer_id AS OfferId,
                               r.rating AS Rating,
                               r.comment AS Comment,
                               r.created_on_utc AS CreatedOnUtc,
                               c.name AS CampaignName
                           FROM campaigns.reviews r
                           INNER JOIN campaigns.campaigns c ON r.campaign_id = c.id
                           WHERE r.id = ANY(@ReviewIds)
                           ORDER BY r.created_on_utc
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { ReviewIds = reviewIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<ReviewSeedData> reviews = await connection.QueryAsync<ReviewSeedData>(commandDefinition);
        return reviews.AsList();
    }

    private static async
        Task<List<(Guid CampaignId, Guid CreatorId, Guid ClientId, Guid OfferId, DateTimeOffset CompletedOnUtc, string
            CampaignName)>> GetCompletedCampaignsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            IReadOnlyList<Guid> campaignIds,
            CancellationToken cancellationToken = default
        )
    {
        const string sql = """
                           SELECT
                               id AS campaign_id,
                               creator_id,
                               client_id,
                               offer_id,
                               completed_on_utc,
                               name AS campaign_name
                           FROM campaigns.campaigns
                           WHERE id = ANY(@CampaignIds) AND status = 4
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { CampaignIds = campaignIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<(Guid CampaignId, Guid CreatorId, Guid ClientId, Guid OfferId, DateTimeOffset CompletedOnUtc, string
            CampaignName)> campaigns
            = await connection.QueryAsync<(Guid, Guid, Guid, Guid, DateTimeOffset, string)>(commandDefinition);

        return campaigns.AsList();
    }

    private static List<ReviewSeedData> GenerateSeedData(
        List<(Guid CampaignId, Guid CreatorId, Guid ClientId, Guid OfferId, DateTimeOffset CompletedOnUtc, string
            CampaignName)> completedCampaigns)
    {
        var faker = new Faker();
        var reviews = new List<ReviewSeedData>(completedCampaigns.Count);

        foreach ((Guid campaignId, Guid creatorId, Guid clientId, Guid offerId, DateTimeOffset completedOnUtc,
                     string campaignName) in completedCampaigns)
        {
            // Rating distribution: mostly 4-5 stars, some 3 stars, rarely 1-2 stars
            int rating = faker.Random.WeightedRandom(_ratingValues, _ratingWeights);

            reviews.Add(new ReviewSeedData
            {
                Id = Guid.NewGuid(),
                CampaignId = campaignId,
                ClientId = clientId,
                CreatorId = creatorId,
                OfferId = offerId,
                Rating = rating,
                Comment = GenerateReviewComment(faker, rating),
                CreatedOnUtc = completedOnUtc.AddDays(faker.Random.Int(1, 7)),
                CampaignName = campaignName
            });
        }

        return reviews;
    }

    private static string GenerateReviewComment(Faker faker, int rating)
    {
        return rating switch
        {
            5 => faker.PickRandom(_fiveStarComments),
            4 => faker.PickRandom(_fourStarComments),
            3 => faker.PickRandom(_threeStarComments),
            2 => faker.PickRandom(_twoStarComments),
            _ => faker.PickRandom(_oneStarComments)
        };
    }

    private static async Task InsertReviewsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<ReviewSeedData> reviews
    )
    {
        const string sql = """
                           INSERT INTO campaigns.reviews
                               (id, campaign_id, client_id, creator_id, offer_id, rating, comment, created_on_utc)
                           VALUES
                               (@Id, @CampaignId, @ClientId, @CreatorId, @OfferId, @Rating, @Comment, @CreatedOnUtc)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            reviews,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }
}
