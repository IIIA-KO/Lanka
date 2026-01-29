using System.Data;
using Bogus;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Serilog;

namespace Lanka.Modules.Campaigns.Infrastructure.Database.Seeders;

public static class CampaignSeeder
{
    private static readonly string[] _currencies = ["USD", "EUR", "UAH"];

    public static async Task<(List<Guid> Ids, List<CampaignSeedData> Data)> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> bloggerIds,
        IReadOnlyList<Guid> offerIds,
        int campaignsPerBlogger
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(bloggerIds);
        ArgumentNullException.ThrowIfNull(offerIds);

        if (!bloggerIds.Any())
        {
            Log.Warning("No blogger IDs provided for campaign seeding");
            return ([], []);
        }

        if (!offerIds.Any())
        {
            Log.Warning("No offer IDs provided for campaign seeding");
            return ([], []);
        }

        List<Guid> existingIds = await GetExistingCampaignIdsAsync(connection, transaction);

        if (existingIds.Count > 0)
        {
            Log.Information("Campaigns already seeded ({Count} exist)", existingIds.Count);
            List<CampaignSeedData> existingData =
                await GetExistingCampaignDataAsync(connection, transaction, existingIds);
            return (existingIds, existingData);
        }

        List<(Guid OfferId, Guid BloggerId)> offerMappings =
            await GetOfferMappingsAsync(connection, transaction, offerIds);

        if (offerMappings.Count == 0)
        {
            Log.Warning("No offer mappings found");
            return ([], []);
        }

        (List<CampaignSeedData> campaigns, List<Guid> campaignIds) = await GenerateSeedDataAsync(
            connection,
            transaction,
            bloggerIds,
            offerMappings,
            campaignsPerBlogger
        );

        await InsertCampaignsAsync(connection, transaction, campaigns);

        Log.Information("Seeded {Count} campaigns for {BloggerCount} bloggers", campaigns.Count, bloggerIds.Count);

        return (campaignIds, campaigns);
    }

    private static async Task<List<Guid>> GetExistingCampaignIdsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default
    )
    {
        const string checkSql = """
                                SELECT COUNT(*)
                                FROM campaigns.campaigns
                                """;

        var commandDefinition = new CommandDefinition(
            checkSql,
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        int existingCount = await connection.ExecuteScalarAsync<int>(commandDefinition);

        if (existingCount == 0)
        {
            return [];
        }

        const string selectSql = """
                                 SELECT id
                                 FROM campaigns.campaigns
                                 ORDER BY id
                                 """;

        var selectCommand = new CommandDefinition(
            selectSql,
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> existingIds = await connection.QueryAsync<Guid>(selectCommand);
        return existingIds.AsList();
    }

    private static async Task<List<CampaignSeedData>> GetExistingCampaignDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> campaignIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT
                               c.id AS Id,
                               c.creator_id AS CreatorId,
                               c.client_id AS ClientId,
                               c.offer_id AS OfferId,
                               c.name AS Name,
                               c.description AS Description,
                               c.scheduled_on_utc AS ScheduledOnUtc,
                               c.status AS Status,
                               c.pended_on_utc AS PendedOnUtc,
                               c.confirmed_on_utc AS ConfirmedOnUtc,
                               c.rejected_on_utc AS RejectedOnUtc,
                               c.done_on_utc AS DoneOnUtc,
                               c.completed_on_utc AS CompletedOnUtc,
                               c.cancelled_on_utc AS CancelledOnUtc,
                               c.price_amount AS PriceAmount,
                               c.price_currency AS PriceCurrency,
                               creator.first_name AS CreatorFirstName,
                               creator.last_name AS CreatorLastName,
                               client.first_name AS ClientFirstName,
                               client.last_name AS ClientLastName
                           FROM campaigns.campaigns c
                           INNER JOIN campaigns.bloggers creator ON c.creator_id = creator.id
                           INNER JOIN campaigns.bloggers client ON c.client_id = client.id
                           WHERE c.id = ANY(@CampaignIds)
                           ORDER BY c.id
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { CampaignIds = campaignIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<CampaignSeedData> campaigns = await connection.QueryAsync<CampaignSeedData>(commandDefinition);
        return campaigns.AsList();
    }

    private static async Task<List<(Guid OfferId, Guid BloggerId)>> GetOfferMappingsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> offerIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT o.id AS offer_id, p.blogger_id
                           FROM campaigns.offers o
                           INNER JOIN campaigns.pacts p ON o.pact_id = p.id
                           WHERE o.id = ANY(@OfferIds)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { OfferIds = offerIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<(Guid OfferId, Guid BloggerId)> mappings =
            await connection.QueryAsync<(Guid, Guid)>(commandDefinition);
        return mappings.AsList();
    }

    private static async Task<(List<CampaignSeedData> campaigns, List<Guid> campaignIds)> GenerateSeedDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> bloggerIds,
        List<(Guid OfferId, Guid BloggerId)> offerMappings,
        int campaignsPerBlogger,
        CancellationToken cancellationToken = default
    )
    {
        List<(Guid Id, string FirstName, string LastName)> bloggerNames =
            await GetBloggerNamesAsync(connection, transaction, bloggerIds, cancellationToken);
        var bloggerNameMap = bloggerNames.ToDictionary(b => b.Id);

        var faker = new Faker();
        var campaignIds = new List<Guid>();
        var campaigns = new List<CampaignSeedData>();

        var bloggerIdSet = new HashSet<Guid>(bloggerIds);

        foreach (Guid creatorId in bloggerIds)
        {
            for (int i = 0; i < campaignsPerBlogger; i++)
            {
                (Guid OfferId, Guid BloggerId) selectedOffer = offerMappings
                    .Where(o => o.BloggerId != creatorId && bloggerIdSet.Contains(o.BloggerId))
                    .OrderBy(_ => faker.Random.Int())
                    .FirstOrDefault();

                if (selectedOffer == default)
                {
                    continue;
                }

                var campaignId = Guid.NewGuid();
                campaignIds.Add(campaignId);

                // Randomly assign status: Pending=0, Confirmed=1, Rejected=2, Done=3, Completed=4, Cancelled=5
                int status = faker.PickRandom(0, 0, 0, 1, 1, 2, 3, 4, 5); // More pending and confirmed

                DateTimeOffset scheduledOnUtc = faker.Date.FutureOffset(30);
                DateTimeOffset pendedOnUtc = DateTimeOffset.UtcNow.AddDays(-faker.Random.Int(5, 60));

                (Guid Id, string FirstName, string LastName) creatorName = bloggerNameMap[creatorId];
                (Guid Id, string FirstName, string LastName) clientName = bloggerNameMap[selectedOffer.BloggerId];

                campaigns.Add(new CampaignSeedData
                {
                    Id = campaignId,
                    CreatorId = creatorId,
                    ClientId = selectedOffer.BloggerId,
                    OfferId = selectedOffer.OfferId,
                    Name = faker.Commerce.ProductName(),
                    Description = faker.Lorem.Sentence(10),
                    ScheduledOnUtc = scheduledOnUtc,
                    Status = status,
                    PendedOnUtc = pendedOnUtc,
                    ConfirmedOnUtc = status >= 1 ? pendedOnUtc.AddDays(faker.Random.Int(1, 3)) : null,
                    RejectedOnUtc = status == 2 ? pendedOnUtc.AddDays(faker.Random.Int(1, 3)) : null,
                    DoneOnUtc = status >= 3 ? scheduledOnUtc.AddDays(faker.Random.Int(1, 5)) : null,
                    CompletedOnUtc = status == 4 ? scheduledOnUtc.AddDays(faker.Random.Int(6, 10)) : null,
                    CancelledOnUtc = status == 5 ? pendedOnUtc.AddDays(faker.Random.Int(1, 3)) : null,
                    PriceAmount = faker.Random.Decimal(100, 5000),
                    PriceCurrency = faker.PickRandom(_currencies),
                    CreatorFirstName = creatorName.FirstName,
                    CreatorLastName = creatorName.LastName,
                    ClientFirstName = clientName.FirstName,
                    ClientLastName = clientName.LastName
                });
            }
        }

        return (campaigns, campaignIds);
    }

    private static async Task<List<(Guid Id, string FirstName, string LastName)>> GetBloggerNamesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> bloggerIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT id, first_name, last_name
                           FROM campaigns.bloggers
                           WHERE id = ANY(@BloggerIds)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { BloggerIds = bloggerIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<(Guid Id, string FirstName, string LastName)> names =
            await connection.QueryAsync<(Guid, string, string)>(commandDefinition);
        return names.AsList();
    }

    private static async Task InsertCampaignsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<CampaignSeedData> campaigns
    )
    {
        const string sql = """
                           INSERT INTO campaigns.campaigns
                               (id, creator_id, client_id, offer_id, name, description, scheduled_on_utc, status,
                                pended_on_utc, confirmed_on_utc, rejected_on_utc, done_on_utc, completed_on_utc, cancelled_on_utc,
                                price_amount, price_currency)
                           VALUES
                               (@Id, @CreatorId, @ClientId, @OfferId, @Name, @Description, @ScheduledOnUtc, @Status,
                                @PendedOnUtc, @ConfirmedOnUtc, @RejectedOnUtc, @DoneOnUtc, @CompletedOnUtc, @CancelledOnUtc,
                                @PriceAmount, @PriceCurrency)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            campaigns,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }
}
