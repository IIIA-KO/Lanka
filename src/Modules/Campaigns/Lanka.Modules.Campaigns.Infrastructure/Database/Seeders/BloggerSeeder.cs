using System.Data;
using Bogus;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Serilog;

namespace Lanka.Modules.Campaigns.Infrastructure.Database.Seeders;

public static class BloggerSeeder
{
    private static readonly string[] _categories =
    [
        "Fashion and Style",
        "Cooking and Food",
        "Technology",
        "Travel",
        "Fitness",
        "Gaming",
        "Music",
        "Art",
        "Photography",
        "Sports"
    ];

    public static async Task<(List<Guid> Ids, List<BloggerSeedData> Data)> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> userIds,
        List<InstagramAccountSeedData>? instagramAccounts = null
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(userIds);

        if (!userIds.Any())
        {
            Log.Warning("No user IDs provided for blogger seeding");
            return ([], []);
        }

        // Check which users already have blogger profiles (blogger ID = user ID)
        List<Guid> existingBloggerIds = await GetExistingBloggerIdsForUsersAsync(connection, transaction, userIds);

        if (existingBloggerIds.Count == userIds.Count)
        {
            Log.Information("Bloggers already exist for all {Count} users", userIds.Count);
            List<BloggerSeedData> existingData =
                await GetExistingBloggerDataAsync(connection, transaction, existingBloggerIds);
            return (existingBloggerIds, existingData);
        }

        // Get users that don't have blogger profiles yet
        var userIdsWithoutBloggers = userIds.Except(existingBloggerIds).ToList();

        if (userIdsWithoutBloggers.Count == 0)
        {
            Log.Information("All users already have blogger profiles");
            List<BloggerSeedData> existingData =
                await GetExistingBloggerDataAsync(connection, transaction, existingBloggerIds);
            return (existingBloggerIds, existingData);
        }

        Log.Information("Creating blogger profiles for {Count} users (out of {Total} total)",
            userIdsWithoutBloggers.Count, userIds.Count);

        List<(Guid Id, string FirstName, string LastName, string Email, DateOnly BirthDate)> users =
            await GetUsersDataAsync(connection, transaction, userIdsWithoutBloggers);

        if (users.Count == 0)
        {
            Log.Warning("No users found with provided IDs");
            List<BloggerSeedData> existingData =
                await GetExistingBloggerDataAsync(connection, transaction, existingBloggerIds);
            return (existingBloggerIds, existingData);
        }

        (List<BloggerSeedData> bloggers, List<Guid> bloggerIds) = GenerateSeedData(users, instagramAccounts);

        await InsertBloggersAsync(connection, transaction, bloggers);

        Log.Information("Seeded {Count} bloggers", bloggers.Count);

        // Return all blogger IDs (existing + newly created)
        var allBloggerIds = existingBloggerIds.Concat(bloggerIds).ToList();
        List<BloggerSeedData> allData = await GetExistingBloggerDataAsync(connection, transaction, allBloggerIds);

        return (allBloggerIds, allData);
    }

    private static async Task<List<Guid>> GetExistingBloggerIdsForUsersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT id
                           FROM campaigns.bloggers
                           WHERE id = ANY(@UserIds)
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

    private static async Task<List<BloggerSeedData>> GetExistingBloggerDataAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<Guid> bloggerIds,
        CancellationToken cancellationToken = default
    )
    {
        const string sql = """
                           SELECT
                               id AS Id,
                               first_name AS FirstName,
                               last_name AS LastName,
                               email AS Email,
                               birth_date AS BirthDate,
                               bio AS Bio,
                               category_name AS CategoryName,
                               instagram_metadata_username AS InstagramMetadataUsername,
                               instagram_metadata_followers_count AS InstagramMetadataFollowersCount,
                               instagram_metadata_media_count AS InstagramMetadataMediaCount,
                               profile_photo_id AS ProfilePhotoId,
                               profile_photo_uri AS ProfilePhotoUri
                           FROM campaigns.bloggers
                           WHERE id = ANY(@BloggerIds)
                           ORDER BY id
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { BloggerIds = bloggerIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<BloggerSeedData> bloggers = await connection.QueryAsync<BloggerSeedData>(commandDefinition);
        return bloggers.AsList();
    }

    private static async Task<List<(Guid Id, string FirstName, string LastName, string Email, DateOnly BirthDate)>>
        GetUsersDataAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            IReadOnlyList<Guid> userIds,
            CancellationToken cancellationToken = default
        )
    {
        const string sql = """
                           SELECT id, first_name, last_name, email, birth_date
                           FROM users.users
                           WHERE id = ANY(@UserIds)
                           ORDER BY id
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            new { UserIds = userIds.ToArray() },
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<(Guid Id, string FirstName, string LastName, string Email, DateOnly BirthDate)> users =
            await connection.QueryAsync<(Guid, string, string, string, DateOnly)>(commandDefinition);
        return users.AsList();
    }

    private static (List<BloggerSeedData> bloggers, List<Guid> bloggerIds) GenerateSeedData(
        List<(Guid Id, string FirstName, string LastName, string Email, DateOnly BirthDate)> users,
        List<InstagramAccountSeedData>? instagramAccounts)
    {
        var faker = new Faker();
        var bloggerIds = new List<Guid>(users.Count);
        var bloggers = new List<BloggerSeedData>(users.Count);

        // Create a dictionary for quick Instagram account lookup by user ID
        // Filter out accounts with invalid UserId (Guid.Empty) to avoid duplicate key errors
        Dictionary<Guid, InstagramAccountSeedData> igAccountsByUserId =
            instagramAccounts?
                .Where(ig => ig.UserId != Guid.Empty)
                .ToDictionary(ig => ig.UserId, ig => ig)
            ?? [];

        foreach ((Guid Id, string FirstName, string LastName, string Email, DateOnly BirthDate) user in users)
        {
            Guid bloggerId = user.Id;
            bloggerIds.Add(bloggerId);

            // Get Instagram metadata if user has an account
            InstagramAccountSeedData? igAccount = igAccountsByUserId.GetValueOrDefault(user.Id);

            bloggers.Add(new BloggerSeedData
            {
                Id = bloggerId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                Bio = faker.Lorem.Sentence(10),
                CategoryName = faker.PickRandom(_categories),
                InstagramMetadataUsername = igAccount?.MetadataUserName,
                InstagramMetadataFollowersCount = igAccount?.MetadataFollowersCount,
                InstagramMetadataMediaCount = igAccount?.MetadataMediaCount,
                ProfilePhotoId = null,
                ProfilePhotoUri = null
            });
        }

        return (bloggers, bloggerIds);
    }

    private static async Task InsertBloggersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<BloggerSeedData> bloggers
    )
    {
        const string sql = """
                           INSERT INTO campaigns.bloggers
                               (id, first_name, last_name, email, birth_date, bio, category_name,
                                instagram_metadata_username, instagram_metadata_followers_count, instagram_metadata_media_count,
                                profile_photo_id, profile_photo_uri)
                           VALUES
                               (@Id, @FirstName, @LastName, @Email, @BirthDate, @Bio, @CategoryName,
                                @InstagramMetadataUsername, @InstagramMetadataFollowersCount, @InstagramMetadataMediaCount,
                                @ProfilePhotoId, @ProfilePhotoUri)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            bloggers,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }
}
