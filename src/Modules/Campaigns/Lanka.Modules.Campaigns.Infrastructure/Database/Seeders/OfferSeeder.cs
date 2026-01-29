using System.Data;
using Bogus;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Serilog;

namespace Lanka.Modules.Campaigns.Infrastructure.Database.Seeders;

public static class OfferSeeder
{
    private static readonly string[] _offerNames =
    [
        "Instagram Story",
        "Reels Mention",
        "Feed Post",
        "IGTV Video",
        "Live Session",
        "Story Highlight",
        "Product Review",
        "Tutorial Video",
        "Giveaway Post",
        "Carousel Post"
    ];

    private static readonly string[] _currencies = ["USD", "EUR", "UAH"];

    public static async Task<(List<Guid> Ids, List<OfferSeedData> Data)> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> pactIds
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(pactIds);

        if (!pactIds.Any())
        {
            Log.Warning("No pact IDs provided for offer seeding");
            return ([], []);
        }

        // Check which pacts already have offers
        List<Guid> pactsWithOffers = await GetPactsWithOffersAsync(connection, transaction, pactIds);
        List<Guid> existingOfferIds = await GetExistingOfferIdsForPactsAsync(connection, transaction, pactIds);

        if (pactsWithOffers.Count == pactIds.Count)
        {
            Log.Information("Offers already exist for all {Count} pacts", pactIds.Count);
            List<OfferSeedData> existingData = await GetExistingOfferDataAsync(connection, transaction, existingOfferIds);
            return (existingOfferIds, existingData);
        }

        // Get pacts that don't have offers yet
        var pactsWithoutOffers = pactIds.Except(pactsWithOffers).ToList();

        if (pactsWithoutOffers.Count == 0)
        {
            Log.Information("All pacts already have offers");
            List<OfferSeedData> existingData = await GetExistingOfferDataAsync(connection, transaction, existingOfferIds);
            return (existingOfferIds, existingData);
        }

        Log.Information("Creating offers for {Count} pacts (out of {Total} total)",
            pactsWithoutOffers.Count, pactIds.Count);

        (List<OfferSeedData> offers, List<Guid> offerIds) = await GenerateSeedDataAsync(connection, transaction, pactsWithoutOffers);

        await InsertOffersAsync(connection, transaction, offers);

        Log.Information("Seeded {Count} offers for {PactCount} pacts", offers.Count, pactsWithoutOffers.Count);

        // Return all offer IDs (existing + newly created)
        var allOfferIds = existingOfferIds.Concat(offerIds).ToList();
        List<OfferSeedData> allData = await GetExistingOfferDataAsync(connection, transaction, allOfferIds);

        return (allOfferIds, allData);
    }

    private static async Task<List<Guid>> GetPactsWithOffersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> pactIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                          SELECT DISTINCT pact_id
                          FROM campaigns.offers
                          WHERE pact_id = ANY(@PactIds)
                          """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { PactIds = pactIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> pactIdsWithOffers = await connection.QueryAsync<Guid>(commandDefinition);
        return pactIdsWithOffers.AsList();
    }

    private static async Task<List<Guid>> GetExistingOfferIdsForPactsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> pactIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                          SELECT id
                          FROM campaigns.offers
                          WHERE pact_id = ANY(@PactIds)
                          ORDER BY id
                          """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { PactIds = pactIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> existingIds = await connection.QueryAsync<Guid>(commandDefinition);
        return existingIds.AsList();
    }

    private static async Task<List<OfferSeedData>> GetExistingOfferDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> offerIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT
                               o.id AS Id,
                               o.pact_id AS PactId,
                               o.name AS Name,
                               o.description AS Description,
                               o.price_amount AS PriceAmount,
                               o.price_currency AS PriceCurrency,
                               o.last_cooperated_on_utc AS LastCooperatedOnUtc,
                               p.blogger_id AS BloggerId,
                               b.first_name AS BloggerFirstName,
                               b.last_name AS BloggerLastName
                           FROM campaigns.offers o
                           INNER JOIN campaigns.pacts p ON o.pact_id = p.id
                           INNER JOIN campaigns.bloggers b ON p.blogger_id = b.id
                           WHERE o.id = ANY(@OfferIds)
                           ORDER BY o.id
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { OfferIds = offerIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<OfferSeedData> offers = await connection.QueryAsync<OfferSeedData>(commandDefinition);
        return offers.AsList();
    }

    private static async Task<(List<OfferSeedData> offers, List<Guid> offerIds)> GenerateSeedDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> pactIds,
        CancellationToken cancellationToken = default
    )
    {
        // Fetch pact to blogger mapping
        List<(Guid PactId, Guid BloggerId, string BloggerFirstName, string BloggerLastName)> pactBloggers = await GetPactBloggersAsync(connection, transaction, pactIds, cancellationToken);
        var pactBloggerMap = pactBloggers.ToDictionary(pb => pb.PactId);

        var faker = new Faker();
        var offerIds = new List<Guid>();
        var offers = new List<OfferSeedData>();

        foreach (Guid pactId in pactIds)
        {
            (Guid PactId, Guid BloggerId, string BloggerFirstName, string BloggerLastName) pactBlogger = pactBloggerMap[pactId];
            int offersCount = faker.Random.Int(3, 5);
            var usedNames = new HashSet<string>();

            for (int i = 0; i < offersCount; i++)
            {
                var offerId = Guid.NewGuid();
                offerIds.Add(offerId);

                // Get unique offer name for this pact
                string offerName;
                do
                {
                    offerName = faker.PickRandom(_offerNames);
                } while (usedNames.Contains(offerName) && usedNames.Count < _offerNames.Length);
                usedNames.Add(offerName);

                // Randomly decide if this offer has been used before (30% chance)
                DateTimeOffset? lastCooperatedOnUtc = faker.Random.Bool(0.3f)
                    ? DateTimeOffset.UtcNow.AddDays(-faker.Random.Int(30, 180))
                    : null;

                offers.Add(new OfferSeedData
                {
                    Id = offerId,
                    PactId = pactId,
                    Name = offerName,
                    Description = faker.Lorem.Sentence(8),
                    PriceAmount = faker.Random.Decimal(50, 5000),
                    PriceCurrency = faker.PickRandom(_currencies),
                    LastCooperatedOnUtc = lastCooperatedOnUtc,
                    BloggerId = pactBlogger.BloggerId,
                    BloggerFirstName = pactBlogger.BloggerFirstName,
                    BloggerLastName = pactBlogger.BloggerLastName
                });
            }
        }

        return (offers, offerIds);
    }

    private static async Task<List<(Guid PactId, Guid BloggerId, string BloggerFirstName, string BloggerLastName)>> GetPactBloggersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> pactIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT
                               p.id AS pact_id,
                               p.blogger_id,
                               b.first_name AS blogger_first_name,
                               b.last_name AS blogger_last_name
                           FROM campaigns.pacts p
                           INNER JOIN campaigns.bloggers b ON p.blogger_id = b.id
                           WHERE p.id = ANY(@PactIds)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { PactIds = pactIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<(Guid PactId, Guid BloggerId, string BloggerFirstName, string BloggerLastName)> pactBloggers = await connection.QueryAsync<(Guid, Guid, string, string)>(commandDefinition);
        return pactBloggers.AsList();
    }

    private static async Task InsertOffersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<OfferSeedData> offers
    )
    {
        const string sql = """
                           INSERT INTO campaigns.offers
                               (id, pact_id, name, description, price_amount, price_currency, last_cooperated_on_utc)
                           VALUES
                               (@Id, @PactId, @Name, @Description, @PriceAmount, @PriceCurrency, @LastCooperatedOnUtc)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            offers,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }
}
