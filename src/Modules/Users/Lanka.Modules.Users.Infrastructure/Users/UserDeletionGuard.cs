using System.Data.Common;
using Dapper;
using Lanka.Common.Application.Data;
using Lanka.Modules.Users.Application.Abstractions.Data;

namespace Lanka.Modules.Users.Infrastructure.Users;

internal sealed class UserDeletionGuard : IUserDeletionGuard
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UserDeletionGuard(IDbConnectionFactory dbConnectionFactory)
    {
        this._dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> HasActiveCampaignsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT EXISTS (
                SELECT 1
                FROM campaigns.campaigns
                WHERE (client_id = @UserId OR creator_id = @UserId)
                  AND status IN (0, 1, 4)
            )
            """;

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { UserId = userId },
                cancellationToken: cancellationToken));
    }
}
