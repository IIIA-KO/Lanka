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
                bool forceCreator = (m * campaignsPerMonth + i) % 2 == 0;
                CampaignSeedData? campaign = GenerateSingleCampaign(
                    faker, targetBlogger, allBloggers, ownOffers, othersOffers, bloggerNameMap,
                    monthStart, monthEnd, forceCreator);

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
        DateTimeOffset monthEnd,
        bool forceCreator = true
    )
    {
        var otherBloggers = allBloggers.Where(b => b.Id != targetBlogger.Id).ToList();
        if (otherBloggers.Count == 0) { return null; }

        Guid creatorId;
        Guid clientId;
        OfferSeedInfo selectedOffer;

        if (forceCreator)
        {
            if (ownOffers.Count > 0)
            {
                selectedOffer = faker.PickRandom(ownOffers);
            }
            else if (othersOffers.Count > 0)
            {
                selectedOffer = faker.PickRandom(othersOffers);
            }
            else
            {
                return null;
            }
            creatorId = targetBlogger.Id;
            clientId = faker.PickRandom(otherBloggers).Id;
        }
        else
        {
            if (othersOffers.Count == 0) { return null; }
            selectedOffer = faker.PickRandom(othersOffers);
            creatorId = selectedOffer.BloggerId;
            clientId = targetBlogger.Id;
        }

        // 0=Pending 1=Confirmed 2=Rejected 3=Cancelled 4=Done 5=Completed
        int status = faker.PickRandom(0, 0, 1, 1, 2, 3, 4, 4, 5, 5);
        DateTimeOffset scheduledOnUtc = faker.Date.BetweenOffset(monthStart, monthEnd).ToUniversalTime();
        DateTimeOffset pendedOnUtc = scheduledOnUtc.AddDays(-faker.Random.Int(5, 30));
        DateTimeOffset? doneOnUtc = (status == 4 || status == 5)
            ? scheduledOnUtc.AddDays(faker.Random.Int(1, 5))
            : null;

        BloggerSeedInfo creator = bloggerNameMap[creatorId];
        BloggerSeedInfo client = bloggerNameMap[clientId];

        (string? reportContent, string? reportApproach, string? reportNotes, string[]? reportLinks, DateTimeOffset? reportSubmitted)
            = GenerateReport(faker, status, doneOnUtc, faker.Commerce.ProductName());

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
            DoneOnUtc = doneOnUtc,
            CompletedOnUtc = status == 5
                ? scheduledOnUtc.AddDays(faker.Random.Int(6, 10))
                : null,
            CancelledOnUtc = status == 3
                ? pendedOnUtc.AddDays(faker.Random.Int(1, 3))
                : null,
            PriceAmount = selectedOffer.PriceAmount,
            PriceCurrency = selectedOffer.PriceCurrency,
            CreatorFirstName = creator.FirstName,
            CreatorLastName = creator.LastName,
            ClientFirstName = client.FirstName,
            ClientLastName = client.LastName,
            ReportContentDelivered = reportContent,
            ReportApproach = reportApproach,
            ReportNotes = reportNotes,
            ReportPostPermalinks = reportLinks,
            ReportSubmittedOnUtc = reportSubmitted
        };
    }

    private static (string? Content, string? Approach, string? Notes, string[]? Links, DateTimeOffset? SubmittedOnUtc)
        GenerateReport(Faker faker, int status, DateTimeOffset? doneOnUtc, string productName)
    {
        if (status != 4 && status != 5) { return (null, null, null, null, null); }

        string[] contentTemplates =
        [
            $"Published an Instagram Reel showcasing {productName} in a lifestyle setting with voiceover explanation.",
            $"Created a series of 3 Instagram Stories featuring {productName} with interactive poll stickers.",
            $"Produced a carousel post with 6 slides demonstrating the key benefits of {productName}.",
            $"Filmed a short-form video review of {productName} highlighting real daily usage scenarios.",
            $"Wrote an Instagram feed post featuring {productName} integrated naturally into morning routine content.",
        ];

        string[] approachTemplates =
        [
            "Used a lifestyle storytelling angle to make the product feel natural rather than promotional. Engagement was above average.",
            "Focused on authentic user experience — showed real usage scenarios instead of staged photos. Audience responded positively.",
            "Kept the tone conversational and relatable. Avoided overly salesy language. Comments were mostly curiosity and questions about the product.",
            "Leveraged trending audio to boost organic reach on the Reel. Reached approximately 2x the usual impressions.",
            "Used close-up product shots with natural lighting for a premium feel. The post performed well with the fashion-oriented audience segment.",
        ];

        string[] notesTemplates =
        [
            "Delivery was slightly delayed due to product shipment — client was informed in advance.",
            "Would recommend a follow-up story series in 2 weeks to reinforce the message.",
            "The audience asked several questions about pricing — a link in bio would help conversion.",
            null!,
            null!,
        ];

        string[]? links = null;
        if (faker.Random.Bool())
        {
            var raw = new List<string> { $"https://www.instagram.com/p/{faker.Random.AlphaNumeric(11)}/" };
            if (faker.Random.Bool())
            {
                raw.Add($"https://www.instagram.com/reel/{faker.Random.AlphaNumeric(11)}/");
            }
            links = raw.ToArray();
        }

        return (
            faker.PickRandom(contentTemplates),
            faker.PickRandom(approachTemplates),
            faker.PickRandom(notesTemplates),
            links,
            doneOnUtc?.AddHours(faker.Random.Int(1, 6))
        );
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
                                price_amount, price_currency,
                                report_content_delivered, report_approach, report_notes,
                                report_post_permalinks, report_submitted_on_utc)
                           VALUES
                               (@Id, @CreatorId, @ClientId, @OfferId, @Name, @Description, @ScheduledOnUtc, @Status,
                                @PendedOnUtc, @ConfirmedOnUtc, @RejectedOnUtc, @DoneOnUtc, @CompletedOnUtc, @CancelledOnUtc,
                                @PriceAmount, @PriceCurrency,
                                @ReportContentDelivered, @ReportApproach, @ReportNotes,
                                @ReportPostPermalinks, @ReportSubmittedOnUtc)
                           """;

        await connection.ExecuteAsync(sql, campaigns);
    }

    private sealed record BloggerSeedInfo(Guid Id, string FirstName, string LastName);

    private sealed record OfferSeedInfo(Guid OfferId, Guid BloggerId, decimal PriceAmount, string PriceCurrency);
}
