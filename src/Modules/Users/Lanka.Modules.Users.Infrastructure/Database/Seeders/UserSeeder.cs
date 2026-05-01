using System.Data;
using Bogus;
using Dapper;
using Lanka.Common.Contracts.Seeding;
using Lanka.Modules.Users.Infrastructure.Identity.Apis;
using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Lanka.Modules.Users.Infrastructure.Database.Seeders;

public static class UserSeeder
{
    private const string FakeEmailDomain = "@fake.lanka.test";
    private const string FakePassword = "Dev1234!";

    public static async Task<List<UserSeedData>> SeedAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        int fakeUserCount,
        IServiceProvider? serviceProvider = null
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

        (List<UserSeedData> users, List<UserRoleSeedData> userRoles) =
            GenerateSeedData(fakeUserCount);

        if (serviceProvider is not null)
        {
            await CreateKeycloakAccountsAsync(users, serviceProvider);
        }

        await InsertUsersAsync(connection, transaction, users);
        await InsertUserRolesAsync(connection, transaction, userRoles);

        Log.Information("Seeded {Count} fake users with roles (password: {Password})", users.Count, FakePassword);
        LogUserMapping(users);

        return users;
    }

    private static async Task CreateKeycloakAccountsAsync(
        List<UserSeedData> users,
        IServiceProvider serviceProvider
    )
    {
        IKeycloakAdminApi api = serviceProvider.GetRequiredService<IKeycloakAdminApi>();

        for (int i = 0; i < users.Count; i++)
        {
            UserSeedData user = users[i];
            try
            {
                List<UserRepresentation> existing = await api.QueryUsersByEmailAsync(user.Email);
                if (existing.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Debug("Fake user {Email} already exists in Keycloak", user.Email);
                    continue;
                }

                var representation = new UserRepresentation(
                    Username: user.Email,
                    Email: user.Email,
                    FirstName: user.FirstName,
                    LastName: user.LastName,
                    EmailVerified: true,
                    Enabled: true,
                    Credentials: [new CredentialRepresentation("password", FakePassword, false)]
                );

                HttpResponseMessage response = await api.CreateUserAsync(representation);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("Failed to create Keycloak user {Email}: {Status}", user.Email, response.StatusCode);
                    continue;
                }

                string keycloakId = response.Headers.Location!.Segments[^1];
                users[i] = user with { IdentityId = keycloakId };

                Log.Debug("Created Keycloak account for {Email} ({Id})", user.Email, keycloakId);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not create Keycloak account for {Email}, keeping fake identity id", user.Email);
            }
        }
    }

    private static void LogUserMapping(List<UserSeedData> users)
    {
        Log.Information("=== Fake user login credentials (password: {Password}) ===", FakePassword);
        foreach (UserSeedData u in users)
        {
            Log.Information("  {Email}  ({FirstName} {LastName})", u.Email, u.FirstName, u.LastName);
        }
        Log.Information("=== End of fake user list ===");
    }

    private static async Task<List<UserSeedData>> GetExistingUsersAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default
    )
    {
        const string checkSql = $"""
                                SELECT COUNT(*)
                                FROM users.users
                                WHERE email LIKE '%{FakeEmailDomain}'
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

        const string selectSql = $"""
                                 SELECT
                                     id AS Id,
                                     first_name AS FirstName,
                                     last_name AS LastName,
                                     email AS Email,
                                     birth_date AS BirthDate,
                                     identity_id AS IdentityId,
                                     instagram_account_linked_on_utc AS InstagramAccountLinkedOnUtc
                                 FROM users.users
                                 WHERE email LIKE '%{FakeEmailDomain}'
                                 ORDER BY email
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
            string email = $"dev{i + 1:D2}{FakeEmailDomain}";

            users.Add(new UserSeedData
            {
                Id = userId,
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                Email = email,
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
