using System.Data;
using Bogus;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Serilog;

namespace Lanka.Modules.Users.Infrastructure.Database.Seeders;

public static class UserSeeder
{
    public static async Task<List<UserSeedData>> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        int fakeUserCount
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        if (fakeUserCount <= 0)
        {
            Log.Warning("No users requested for seeding (count: {Count})", fakeUserCount);
            return [];
        }

        List<UserSeedData> existingUsers = await GetExistingUsersAsync(connection, transaction);

        if (existingUsers.Count > 0)
        {
            Log.Information("Users already seeded ({Count} fake users exist)", existingUsers.Count);
            return existingUsers;
        }

        (List<UserSeedData> users, List<UserRoleSeedData> userRoles) = GenerateSeedData(fakeUserCount);

        await InsertUsersAsync(connection, transaction, users);
        await InsertUserRolesAsync(connection, transaction, userRoles);

        Log.Information("Seeded {Count} fake users with roles", users.Count);

        return users;
    }

    private static async Task<List<UserSeedData>> GetExistingUsersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default
    )
    {
        const string checkSql = """
                                SELECT COUNT(*)
                                FROM users.users
                                WHERE identity_id LIKE 'fake-%'
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
                                 SELECT
                                     id AS Id,
                                     first_name AS FirstName,
                                     last_name AS LastName,
                                     email AS Email,
                                     birth_date AS BirthDate,
                                     identity_id AS IdentityId,
                                     instagram_account_linked_on_utc AS InstagramAccountLinkedOnUtc
                                 FROM users.users
                                 WHERE identity_id LIKE 'fake-%'
                                 ORDER BY id
                                 """;

        var selectCommand = new CommandDefinition(
            selectSql,
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        IEnumerable<UserSeedData> existingUsers = await connection.QueryAsync<UserSeedData>(selectCommand);
        return existingUsers.AsList();
    }

    private static (List<UserSeedData> users, List<UserRoleSeedData> userRoles)
        GenerateSeedData(int fakeUserCount)
    {
        var faker = new Faker();
        var users = new List<UserSeedData>(fakeUserCount);
        var userRoles = new List<UserRoleSeedData>(fakeUserCount);

        for (int i = 0; i < fakeUserCount; i++)
        {
            var userId = Guid.NewGuid();

            users.Add(new UserSeedData
            {
                Id = userId,
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Email = faker.Internet.Email(),
                BirthDate = DateOnly.FromDateTime(faker.Date.Past(30, DateTime.Now.AddYears(-18))),
                IdentityId = $"fake-{Guid.NewGuid()}",
                InstagramAccountLinkedOnUtc = null
            });

            userRoles.Add(new UserRoleSeedData
            {
                UserId = userId,
                RoleName = "Member"
            });
        }

        return (users, userRoles);
    }

    private static async Task InsertUsersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<UserSeedData> users
    )
    {
        const string sql = """
                           INSERT INTO users.users
                               (id, first_name, last_name, email, birth_date, identity_id, instagram_account_linked_on_utc)
                           VALUES
                               (@Id, @FirstName, @LastName, @Email, @BirthDate, @IdentityId, @InstagramAccountLinkedOnUtc)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            users,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }

    private static async Task InsertUserRolesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        List<UserRoleSeedData> userRoles
    )
    {
        const string sql = """
                           INSERT INTO users.user_roles (user_id, role_name)
                           VALUES (@UserId, @RoleName)
                           """;

        var commandDefinition = new CommandDefinition(
            sql,
            userRoles,
            transaction: transaction
        );

        await connection.ExecuteAsync(commandDefinition);
    }
}
