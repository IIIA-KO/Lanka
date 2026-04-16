using System.Data.Common;
using System.Globalization;
using Bogus;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Common.Contracts.Seeding;
using Lanka.Modules.Campaigns.Application.Abstractions.Campaigns;
using Lanka.Modules.Campaigns.Application.Campaigns.SeedCampaigns;

namespace Lanka.Modules.Campaigns.Infrastructure.Campaigns;

internal sealed class CampaignSeedingService : ICampaignSeedingService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CampaignSeedingService(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<SeedCampaignsResult> SeedAsync(
        Guid bloggerId,
        int campaignsPerMonth,
        CancellationToken cancellationToken = default
    )
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        List<BloggerSeedInfo> allBloggers = await GetAllBloggersAsync(connection, cancellationToken);

        BloggerSeedInfo? targetBlogger = allBloggers.Find(b => b.Id == bloggerId);

        if (targetBlogger is null)
        {
            return new SeedCampaignsResult(0, 0, []);
        }

        List<OfferSeedInfo> offers = await GetOfferMappingsAsync(connection, cancellationToken);

        if (offers.Count == 0)
        {
            return new SeedCampaignsResult(0, 1, []);
        }

        (List<CampaignSeedData> campaigns, List<string> months) =
            GenerateCampaigns(targetBlogger, allBloggers, offers, campaignsPerMonth);

        if (campaigns.Count > 0)
        {
            await InsertCampaignsAsync(connection, campaigns);
        }

        return new SeedCampaignsResult(campaigns.Count, 1, months);
    }

    private static async Task<List<BloggerSeedInfo>> GetAllBloggersAsync(
        DbConnection connection,
        CancellationToken cancellationToken
    )
    {
        const string sql = "SELECT id AS Id, first_name AS FirstName, last_name AS LastName FROM campaigns.bloggers";

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        IEnumerable<BloggerSeedInfo> bloggers = await connection.QueryAsync<BloggerSeedInfo>(command);
        return bloggers.AsList();
    }

    private static async Task<List<OfferSeedInfo>> GetOfferMappingsAsync(
        DbConnection connection,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
                           SELECT o.id AS OfferId, p.blogger_id AS BloggerId,
                                  o.price_amount AS PriceAmount, o.price_currency AS PriceCurrency
                           FROM campaigns.offers o
                           INNER JOIN campaigns.pacts p ON o.pact_id = p.id
                           """;

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        IEnumerable<OfferSeedInfo> mappings = await connection.QueryAsync<OfferSeedInfo>(command);
        return mappings.AsList();
    }

    private static (List<CampaignSeedData> Campaigns, List<string> Months) GenerateCampaigns(
        BloggerSeedInfo targetBlogger,
        List<BloggerSeedInfo> allBloggers,
        List<OfferSeedInfo> offers,
        int campaignsPerMonth
    )
    {
        var bloggerNameMap = allBloggers.ToDictionary(b => b.Id);
        var faker = new Faker();
        var campaigns = new List<CampaignSeedData>();
        var months = new List<string>();

        var ownOffers = offers.Where(o => o.BloggerId == targetBlogger.Id).ToList();
        var othersOffers = offers.Where(o => o.BloggerId != targetBlogger.Id).ToList();

        if (ownOffers.Count == 0 && othersOffers.Count == 0)
        {
            return (campaigns, months);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        for (int m = 0; m < 3; m++)
        {
            DateTimeOffset monthStart = new DateTimeOffset(
                now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(m);
            DateTimeOffset monthEnd = monthStart.AddMonths(1).AddTicks(-1);
            months.Add(monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture));

            for (int i = 0; i < campaignsPerMonth; i++)
            {
                CampaignSeedData? campaign = GenerateSingleCampaign(
                    faker, targetBlogger, allBloggers, ownOffers, othersOffers, bloggerNameMap,
                    monthStart, monthEnd);

                if (campaign is not null)
                {
                    campaigns.Add(campaign);
                }
            }
        }

        return (campaigns, months);
    }

    private static CampaignSeedData? GenerateSingleCampaign(
        Faker faker,
        BloggerSeedInfo targetBlogger,
        List<BloggerSeedInfo> allBloggers,
        List<OfferSeedInfo> ownOffers,
        List<OfferSeedInfo> othersOffers,
        Dictionary<Guid, BloggerSeedInfo> bloggerNameMap,
        DateTimeOffset monthStart,
        DateTimeOffset monthEnd
    )
    {
        bool asCreator = faker.Random.Double() < 0.7 && ownOffers.Count > 0;

        Guid creatorId;
        Guid clientId;
        OfferSeedInfo selectedOffer;

        if (asCreator)
        {
            selectedOffer = faker.PickRandom(ownOffers);
            creatorId = targetBlogger.Id;
            clientId = faker.PickRandom(allBloggers.Where(b => b.Id != targetBlogger.Id).ToList()).Id;
        }
        else if (othersOffers.Count > 0)
        {
            selectedOffer = faker.PickRandom(othersOffers);
            creatorId = selectedOffer.BloggerId;
            clientId = targetBlogger.Id;
        }
        else
        {
            return null;
        }

        int status = faker.PickRandom(0, 0, 0, 1, 1, 1, 2, 3, 4, 5);
        DateTimeOffset scheduledOnUtc = faker.Date.BetweenOffset(monthStart, monthEnd).ToUniversalTime();
        DateTimeOffset pendedOnUtc = scheduledOnUtc.AddDays(-faker.Random.Int(5, 30));

        BloggerSeedInfo creator = bloggerNameMap[creatorId];
        BloggerSeedInfo client = bloggerNameMap[clientId];

        return new CampaignSeedData
        {
            Id = Guid.NewGuid(),
            CreatorId = creatorId,
            ClientId = clientId,
            OfferId = selectedOffer.OfferId,
            Name = faker.Commerce.ProductName(),
            Description = faker.Lorem.Sentence(10),
            ScheduledOnUtc = scheduledOnUtc,
            Status = status,
            PendedOnUtc = pendedOnUtc,
            ConfirmedOnUtc = status >= 1 && status != 2
                ? pendedOnUtc.AddDays(faker.Random.Int(1, 3))
                : null,
            RejectedOnUtc = status == 2
                ? pendedOnUtc.AddDays(faker.Random.Int(1, 3))
                : null,
            DoneOnUtc = status >= 3 && status != 5
                ? scheduledOnUtc.AddDays(faker.Random.Int(1, 5))
                : null,
            CompletedOnUtc = status == 4
                ? scheduledOnUtc.AddDays(faker.Random.Int(6, 10))
                : null,
            CancelledOnUtc = status == 5
                ? pendedOnUtc.AddDays(faker.Random.Int(1, 3))
                : null,
            PriceAmount = selectedOffer.PriceAmount,
            PriceCurrency = selectedOffer.PriceCurrency,
            CreatorFirstName = creator.FirstName,
            CreatorLastName = creator.LastName,
            ClientFirstName = client.FirstName,
            ClientLastName = client.LastName
        };
    }

    private static async Task InsertCampaignsAsync(
        DbConnection connection,
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

        await connection.ExecuteAsync(sql, campaigns);
    }

    private sealed record BloggerSeedInfo(Guid Id, string FirstName, string LastName);

    private sealed record OfferSeedInfo(Guid OfferId, Guid BloggerId, decimal PriceAmount, string PriceCurrency);
}
