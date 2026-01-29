using System.Data.Common;
using Dapper;
using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.Index;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Serilog;

namespace Lanka.Modules.Analytics.Infrastructure.Database.Seeders;

public static class AnalyticsElasticsearchSeeder
{
    public static async Task SeedAsync(
        ISearchIndexService searchIndexService,
        DbConnection connection,
        List<Guid> igAccountIds
    )
    {
        ArgumentNullException.ThrowIfNull(searchIndexService);
        ArgumentNullException.ThrowIfNull(connection);

        Log.Information("Seeding Analytics module entities to Elasticsearch");

        Log.Information("Creating search documents for {Count} Instagram accounts", igAccountIds.Count);
        List<SearchDocument> igAccountDocs = await CreateInstagramAccountDocumentsAsync(connection, igAccountIds);

        await IndexDocumentsAsync(searchIndexService, igAccountDocs);

        Log.Information("Analytics module Elasticsearch seeding completed");
    }

    private static async Task IndexDocumentsAsync(
        ISearchIndexService searchIndexService,
        List<SearchDocument> documents
    )
    {
        const int batchSize = 1000;

        for (int i = 0; i < documents.Count; i += batchSize)
        {
            List<SearchDocument> batch = documents.Skip(i).Take(batchSize).ToList();
            Result result = await searchIndexService.IndexDocumentsAsync(batch);

            if (result.IsFailure)
            {
                Log.Warning("Failed to index batch {BatchNumber}: {Error}",
                    i / batchSize + 1, result.Error.Description);
            }
        }
    }

    private static async Task<List<SearchDocument>> CreateInstagramAccountDocumentsAsync(
        DbConnection connection,
        List<Guid> igAccountIds
    )
    {
        const string sql = """
            SELECT
                id,
                username,
                followers_count,
                media_count,
                biography
            FROM analytics.instagram_accounts
            WHERE id = ANY(@IgAccountIds)
            """;

        var commandDefinition = new CommandDefinition(sql, new { IgAccountIds = igAccountIds.ToArray() });
        List<InstagramAccountDataDto> accounts = (await connection.QueryAsync<InstagramAccountDataDto>(commandDefinition)).ToList();

        var documents = new List<SearchDocument>();

        foreach (InstagramAccountDataDto account in accounts)
        {
            string title = $"@{account.Username}";
            string content = $"Instagram Account | Followers: {account.FollowersCount} | Posts: {account.MediaCount}";

            if (!string.IsNullOrWhiteSpace(account.Biography))
            {
                content += $" | {account.Biography}";
            }

            var tags = new List<string> { "Instagram", "Social Media" };

            var metadata = new Dictionary<string, object>
            {
                ["username"] = account.Username,
                ["followersCount"] = account.FollowersCount,
                ["mediaCount"] = account.MediaCount
            };

            if (!string.IsNullOrWhiteSpace(account.Biography))
            {
                metadata["biography"] = account.Biography;
            }

            Result<SearchDocument> result = SearchDocument.Create(
                sourceEntityId: account.Id,
                type: SearchableItemType.InstagramAccount,
                title: title,
                content: content,
                tags: tags,
                metadata: metadata
            );

            if (result.IsSuccess)
            {
                documents.Add(result.Value);
            }
            else
            {
                Log.Warning("Failed to create search document for Instagram account {AccountId}: {Error}",
                    account.Id, result.Error.Description);
            }
        }

        return documents;
    }

    private sealed class InstagramAccountDataDto
    {
        public Guid Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public int FollowersCount { get; init; }
        public int MediaCount { get; init; }
        public string? Biography { get; init; }
    }
}
