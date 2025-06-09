using System.Data.Common;
using System.Reflection;
using Dapper;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Data;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.UpdateAccount;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.CheckTokens;

[DisallowConcurrentExecution]
internal sealed class CheckTokensJob : IJob
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly TokenOptions _tokenOptions;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<CheckTokensJob> _logger;

    public CheckTokensJob(
        IDateTimeProvider dateTimeProvider,
        IOptions<TokenOptions> tokenOptions,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<CheckTokensJob> logger
    )
    {
        this._dateTimeProvider = dateTimeProvider;
        this._tokenOptions = tokenOptions.Value;
        this._dbConnectionFactory = dbConnectionFactory;
        this._logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        this._logger.LogInformation("Beginning CheckTokensJob execution");

        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        IReadOnlyList<TokenResponse> tokens = await this.GetTokensAsync(connection, transaction);

        foreach (TokenResponse token in tokens)
        {
            Exception? exception = null;

            try
            {
                // Notification logic for token checking will be added here.
            }
            catch (Exception caughtException)
            {
                this._logger.LogError(caughtException,
                    "An exception occurred while checking token {TokenId}",
                    token.Id
                );

                exception = caughtException;
            }

            await this.UpdateTokenAsync(connection, transaction, token, exception);
        }

        await transaction.CommitAsync();

        this._logger.LogInformation("Finished CheckTokensJob execution");
    }

    private async Task<IReadOnlyList<TokenResponse>> GetTokensAsync(
        DbConnection connection,
        DbTransaction transaction
    )
    {
        string sql = $"""
                      SELECT
                          id AS {nameof(TokenResponse.Id)},
                          user_id AS {nameof(TokenResponse.UserId)},
                          access_token AS {nameof(TokenResponse.AccessToken)},
                          expires_at_utc AS {nameof(TokenResponse.ExpiresAtUtc)}
                      FROM analytics.tokens
                      WHERE expires_at_utc <= CURRENT_DATE + INTERVAL '{this._tokenOptions.RenewalThresholdInDays} days'
                      ORDER BY expires_at_utc ASC
                      """;

        IEnumerable<TokenResponse> tokens = await connection.QueryAsync<TokenResponse>(
            sql,
            transaction: transaction
        );

        return tokens.ToList();
    }

    private async Task UpdateTokenAsync(
        DbConnection connection,
        DbTransaction transaction,
        TokenResponse token,
        Exception? exception
    )
    {
        const string sql =
            """
            UPDATE analytics.tokens
            SET
                last_checked_at_utc = @LastCheckedAtUtc,
                error_message = @ErrorMessage
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                LastCheckedAtUtc = this._dateTimeProvider.UtcNow,
                ErrorMessage = exception?.Message,
                token.Id
            },
            transaction: transaction
        );
    }
}
