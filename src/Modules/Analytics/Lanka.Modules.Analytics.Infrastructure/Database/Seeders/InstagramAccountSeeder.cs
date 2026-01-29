using System.Data;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Lanka.Modules.Analytics.Infrastructure.Encryption;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;
using Serilog;

namespace Lanka.Modules.Analytics.Infrastructure.Database.Seeders;

public static class InstagramAccountSeeder
{
    public static async Task<(List<Guid> Ids, List<InstagramAccountSeedData> Data)> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<UserSeedData> users,
        EncryptionService encryptionService
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(users);

        if (!users.Any())
        {
            Log.Warning("No users provided for Instagram account seeding");
            return ([], []);
        }

        var userIds = users.Select(u => u.Id).ToList();

        // Check which users already have Instagram accounts
        List<Guid> existingAccountIds = await GetExistingAccountIdsForUsersAsync(connection, transaction, userIds);

        if (existingAccountIds.Count == users.Count)
        {
            Log.Information("Instagram accounts already seeded for all {Count} users", users.Count);
            List<InstagramAccountSeedData> existingData = await GetExistingAccountDataAsync(connection, transaction, existingAccountIds);
            return (existingAccountIds, existingData);
        }

        // Get users that don't have accounts yet
        List<Guid> userIdsWithAccounts = await GetUserIdsWithAccountsAsync(connection, transaction, userIds);
        var usersWithoutAccounts = users
            .Where(u => !userIdsWithAccounts.Contains(u.Id))
            .ToList();

        if (usersWithoutAccounts.Count == 0)
        {
            Log.Information("All users already have Instagram accounts");
            List<InstagramAccountSeedData> existingData = await GetExistingAccountDataAsync(connection, transaction, existingAccountIds);
            return (existingAccountIds, existingData);
        }

        Log.Information("Seeding Instagram accounts for {Count} users (out of {Total} total)",
            usersWithoutAccounts.Count, users.Count);

        (List<InstagramAccountSeedData> accounts, List<InstagramTokenSeedData> tokens, List<Guid> accountIds) = GenerateSeedData(usersWithoutAccounts, encryptionService);

        await InsertAccountsAsync(connection, transaction, accounts);
        await InsertTokensAsync(connection, transaction, tokens);

        Log.Information("Seeded {Count} Instagram accounts and tokens", accounts.Count);

        // Return all account IDs (existing + newly created)
        var allAccountIds = existingAccountIds.Concat(accountIds).ToList();
        List<InstagramAccountSeedData> allData = await GetExistingAccountDataAsync(connection, transaction, allAccountIds);

        return (allAccountIds, allData);
    }

    private static async Task<List<Guid>> GetExistingAccountIdsForUsersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                          SELECT id
                          FROM analytics.accounts
                          WHERE user_id = ANY(@UserIds)
                          ORDER BY id
                          """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { UserIds = userIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> existingIds = await connection.QueryAsync<Guid>(commandDefinition);
        return existingIds.AsList();
    }

    private static async Task<List<Guid>> GetUserIdsWithAccountsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                          SELECT user_id
                          FROM analytics.accounts
                          WHERE user_id = ANY(@UserIds)
                          """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { UserIds = userIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<Guid> userIdsWithAccounts = await connection.QueryAsync<Guid>(commandDefinition);
        return userIdsWithAccounts.AsList();
    }

    private static async Task<List<InstagramAccountSeedData>> GetExistingAccountDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> accountIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT
                               id AS Id,
                               user_id AS UserId,
                               email AS Email,
                               facebook_page_id AS FacebookPageId,
                               advertisement_account_id AS AdvertisementAccountId,
                               metadata_id AS MetadataId,
                               metadata_ig_id AS MetadataIgId,
                               metadata_user_name AS MetadataUserName,
                               metadata_followers_count AS MetadataFollowersCount,
                               metadata_media_count AS MetadataMediaCount,
                               last_updated_at_utc AS LastUpdatedAtUtc
                           FROM analytics.accounts
                           WHERE id = ANY(@AccountIds)
                           ORDER BY id
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { AccountIds = accountIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<InstagramAccountSeedData> accounts = await connection.QueryAsync<InstagramAccountSeedData>(commandDefinition);
        return accounts.AsList();
    }

    private static (List<InstagramAccountSeedData> accounts, List<InstagramTokenSeedData> tokens, List<Guid> accountIds)
        GenerateSeedData(List<UserSeedData> users, EncryptionService encryptionService)
    {
        var accountIds = new List<Guid>(users.Count);
        var accounts = new List<InstagramAccountSeedData>(users.Count);
        var tokens = new List<InstagramTokenSeedData>(users.Count);

        foreach (UserSeedData user in users)
        {
            var accountId = Guid.NewGuid();
            accountIds.Add(accountId);

            (string facebookPageId, string adAccountId, string metadataId, long metadataIgId,
                string userName, int followersCount, int mediaCount) = FakeInstagramDataGenerator.GenerateAccountMetadata();

            accounts.Add(new InstagramAccountSeedData
            {
                Id = accountId,
                UserId = user.Id,
                Email = user.Email,
                FacebookPageId = facebookPageId,
                AdvertisementAccountId = adAccountId,
                MetadataId = metadataId,
                MetadataIgId = metadataIgId,
                MetadataUserName = userName,
                MetadataFollowersCount = followersCount,
                MetadataMediaCount = mediaCount,
                LastUpdatedAtUtc = null
            });

            // Generate fake token with recognizable prefix, then encrypt it
            string plainTextToken = $"fake_token_{Guid.NewGuid()}";
            string encryptedToken = encryptionService.Encrypt(plainTextToken);

            tokens.Add(new InstagramTokenSeedData
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                InstagramAccountId = accountId,
                AccessToken = encryptedToken,
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMonths(6)
            });
        }

        return (accounts, tokens, accountIds);
    }

    private static async Task InsertAccountsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<InstagramAccountSeedData> accounts
    )
    {
        const string sql = """
                           INSERT INTO analytics.accounts
                               (id, user_id, email, facebook_page_id, advertisement_account_id,
                                metadata_id, metadata_ig_id, metadata_user_name,
                                metadata_followers_count, metadata_media_count, last_updated_at_utc)
                           VALUES
                               (@Id, @UserId, @Email, @FacebookPageId, @AdvertisementAccountId,
                                @MetadataId, @MetadataIgId, @MetadataUserName,
                                @MetadataFollowersCount, @MetadataMediaCount, @LastUpdatedAtUtc)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            accounts,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }

    private static async Task InsertTokensAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<InstagramTokenSeedData> tokens
    )
    {
        const string sql = """
                           INSERT INTO analytics.tokens
                               (id, user_id, instagram_account_id, access_token, expires_at_utc)
                           VALUES
                               (@Id, @UserId, @InstagramAccountId, @AccessToken, @ExpiresAtUtc)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            tokens,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }
}
