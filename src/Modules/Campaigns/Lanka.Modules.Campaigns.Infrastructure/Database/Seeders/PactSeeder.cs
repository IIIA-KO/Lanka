using System.Data;
using Bogus;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Serilog;

namespace Lanka.Modules.Campaigns.Infrastructure.Database.Seeders;

public static class PactSeeder
{
    public static async Task<(List<Guid> Ids, List<PactSeedData> Data)> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> bloggerIds
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(bloggerIds);

        if (!bloggerIds.Any())
        {
            Log.Warning("No blogger IDs provided for pact seeding");
            return ([], []);
        }

        // Check which bloggers already have pacts (pact ID = blogger ID)
        List<Guid> existingPactIds = await GetExistingPactIdsForBloggersAsync(connection, transaction, bloggerIds);

        if (existingPactIds.Count == bloggerIds.Count)
        {
            Log.Information("Pacts already exist for all {Count} bloggers", bloggerIds.Count);
            List<PactSeedData> existingData = await GetExistingPactDataAsync(connection, transaction, existingPactIds);
            return (existingPactIds, existingData);
        }

        // Get bloggers that don't have pacts yet
        var bloggersWithoutPacts = bloggerIds.Except(existingPactIds).ToList();

        if (bloggersWithoutPacts.Count == 0)
        {
            Log.Information("All bloggers already have pacts");
            List<PactSeedData> existingData = await GetExistingPactDataAsync(connection, transaction, existingPactIds);
            return (existingPactIds, existingData);
        }

        Log.Information("Creating pacts for {Count} bloggers (out of {Total} total)",
            bloggersWithoutPacts.Count, bloggerIds.Count);

        (List<PactSeedData> pacts, List<Guid> pactIds) =
            await GenerateSeedDataAsync(connection, transaction, bloggersWithoutPacts);

        await InsertPactsAsync(connection, transaction, pacts);

        Log.Information("Seeded {Count} pacts", pacts.Count);

        // Return all pact IDs (existing + newly created)
        var allPactIds = existingPactIds.Concat(pactIds).ToList();
        List<PactSeedData> allData = await GetExistingPactDataAsync(connection, transaction, allPactIds);

        return (allPactIds, allData);
    }

    private static async Task<List<Guid>> GetExistingPactIdsForBloggersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> bloggerIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                          SELECT id
                          FROM campaigns.pacts
                          WHERE blogger_id = ANY(@BloggerIds)
                          ORDER BY id
                          """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { BloggerIds = bloggerIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> existingIds = await connection.QueryAsync<Guid>(commandDefinition);
        return existingIds.AsList();
    }

    private static async Task<List<PactSeedData>> GetExistingPactDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> pactIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT
                               p.id AS Id,
                               p.blogger_id AS BloggerId,
                               p.content AS Content,
                               p.last_updated_on_utc AS LastUpdatedOnUtc,
                               b.first_name AS BloggerFirstName,
                               b.last_name AS BloggerLastName
                           FROM campaigns.pacts p
                           INNER JOIN campaigns.bloggers b ON p.blogger_id = b.id
                           WHERE p.id = ANY(@PactIds)
                           ORDER BY p.id
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { PactIds = pactIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<PactSeedData> pacts = await connection.QueryAsync<PactSeedData>(commandDefinition);
        return pacts.AsList();
    }

    private static async Task<(List<PactSeedData> pacts, List<Guid> pactIds)> GenerateSeedDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> bloggerIds,
        CancellationToken cancellationToken = default
    )
    {
        // Fetch blogger names
        List<(Guid Id, string FirstName, string LastName)> bloggerNames =
            await GetBloggerNamesAsync(connection, transaction, bloggerIds, cancellationToken);
        var bloggerNameMap = bloggerNames.ToDictionary(b => b.Id);

        var faker = new Faker();
        var pactIds = new List<Guid>(bloggerIds.Count);
        var pacts = new List<PactSeedData>(bloggerIds.Count);

        foreach (Guid bloggerId in bloggerIds)
        {
            var pactId = Guid.NewGuid();
            pactIds.Add(pactId);

            (_, string firstName, string lastName) = bloggerNameMap[bloggerId];

            pacts.Add(new PactSeedData
            {
                Id = pactId,
                BloggerId = bloggerId,
                Content = faker.Lorem.Paragraph(3),
                LastUpdatedOnUtc = DateTimeOffset.UtcNow.AddDays(-faker.Random.Int(1, 30)),
                BloggerFirstName = firstName,
                BloggerLastName = lastName
            });
        }

        return (pacts, pactIds);
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

    private static async Task InsertPactsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<PactSeedData> pacts
    )
    {
        const string sql = """
                           INSERT INTO campaigns.pacts
                               (id, blogger_id, content, last_updated_on_utc)
                           VALUES
                               (@Id, @BloggerId, @Content, @LastUpdatedOnUtc)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            pacts,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }
}
