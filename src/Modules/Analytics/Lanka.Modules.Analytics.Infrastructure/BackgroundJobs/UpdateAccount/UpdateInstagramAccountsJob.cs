using System.Data.Common;
using System.Threading.RateLimiting;
using Dapper;
using Lanka.Common.Application.Clock;
using Lanka.Common.Application.Data;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.UpdateAccount;

[DisallowConcurrentExecution]
internal sealed class UpdateInstagramAccountsJob : IJob
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly InstagramOptions _instagramOptions;
    private readonly IInstagramAccountsService _instagramAccountsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RateLimiter _rateLimiter;
    private readonly ILogger<UpdateInstagramAccountsJob> _logger;

    public UpdateInstagramAccountsJob(
        IDbConnectionFactory dbConnectionFactory,
        IDateTimeProvider dateTimeProvider,
        IOptions<InstagramOptions> instagramOptions,
        IInstagramAccountsService instagramAccountsService,
        IUnitOfWork unitOfWork,
        RateLimiter rateLimiter,
        ILogger<UpdateInstagramAccountsJob> logger
    )
    {
        this._dbConnectionFactory = dbConnectionFactory;
        this._dateTimeProvider = dateTimeProvider;
        this._instagramOptions = instagramOptions.Value;
        this._instagramAccountsService = instagramAccountsService;
        this._unitOfWork = unitOfWork;
        this._rateLimiter = rateLimiter;
        this._logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        this._logger.LogInformation("Beginning UpdateInstagramAccountsJob execution");

        await using DbConnection connection = await this._dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        IReadOnlyList<InstagramAccountResponse> instagramAccounts =
            await this.GetInstagramAccountsAsync(connection, transaction);

        int successCount = 0;
        int failureCount = 0;
        int rateLimitedCount = 0;

        foreach (InstagramAccountResponse account in instagramAccounts)
        {
            using RateLimitLease lease = await this._rateLimiter.AcquireAsync(
                permitCount: 1,
                cancellationToken: context.CancellationToken
            );

            if (!lease.IsAcquired)
            {
                this._logger.LogWarning(
                    "Rate limit reached, stop batch processing. Processed {SuccessCount}, successful, {FailureCount} failed, {RateLimitedCount} rate limited",
                    successCount, failureCount, rateLimitedCount
                );

                rateLimitedCount++;
                break;
            }

            try
            {
                await this.UpdateInstagramAccountMetadataAsync(connection, transaction, account);
                await this._unitOfWork.SaveChangesAsync();

                successCount++;

                this._logger.LogInformation("Successfully updated account {AccountId}", account.Id);
            }
            catch (HttpRequestException exception)
                when (exception.Message.Contains("rate limit"))
            {
                this._logger.LogWarning(exception, "Rate limit exceeded for account {AccountId}", account.Id);
                rateLimitedCount++;
                break;
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception,
                    "Exception occurred while processing Instagram account {AccountId}", account.Id);

                await transaction.RollbackAsync();
            }
        }

        await transaction.CommitAsync();

        this._logger.LogInformation("Completed UpdateInstagramAccountsJob execution");
    }

    private async Task<IReadOnlyList<InstagramAccountResponse>> GetInstagramAccountsAsync(
        DbConnection connection,
        DbTransaction transaction
    )
    {
        string sql = $"""
                      SELECT
                          id as {nameof(InstagramAccountResponse.Id)},
                          user_id as {nameof(InstagramAccountResponse.UserId)},
                          metadata_id as {nameof(InstagramAccountResponse.MetadataId)},
                          metadata_ig_id as {nameof(InstagramAccountResponse.MetadataIgId)},
                          metadata_user_name as {nameof(InstagramAccountResponse.MetadataUserName)},
                          metadata_followers_count as {nameof(InstagramAccountResponse.MetadataFollowersCount)},
                          metadata_media_count as {nameof(InstagramAccountResponse.MetadataMediaCount)}
                      FROM analytics.instagram_accounts
                      WHERE
                          last_updated_at_utc IS NULL
                            OR last_updated_at_utc <= CURRENT_DATE - INTERVAL '{this._instagramOptions.RenewalThresholdInDays} day'
                      ORDER BY last_updated_at_utc ASC NULLS FIRST
                      LIMIT {this._instagramOptions.BatchSize}
                      """;

        IEnumerable<InstagramAccountResponse> instagramAccounts =
            await connection.QueryAsync<InstagramAccountResponse>(sql, transaction: transaction);

        return instagramAccounts.ToList();
    }

    private async Task UpdateInstagramAccountMetadataAsync(
        DbConnection connection,
        DbTransaction transaction,
        InstagramAccountResponse account
    )
    {
        this._logger.LogInformation(
            "Updating metadata for Instagram account {AccountId} at {Time}",
            account.Id, this._dateTimeProvider.UtcNow
        );

        Guid userId = await connection.QueryFirstOrDefaultAsync<Guid>(
            "SELECT user_id FROM analytics.instagram_accounts WHERE id = @Id",
            new { account.Id },
            transaction: transaction
        );

        TokenResponse? token = await connection.QueryFirstOrDefaultAsync<TokenResponse>(
            $"""
             SELECT
             FROM analytics.tokens
             WHERE user_id = @UserId
             """,
            new { UserId = userId },
            transaction: transaction
        );

        if (token is null)
        {
            this._logger.LogWarning(
                "No token found for user associated with Instagram account {AccountId}",
                account.Id
            );

            return;
        }

        Result<InstagramUserInfo> instagramAccountResult = await this._instagramAccountsService
            .GetUserInfoAsync(token.AccessToken);

        if (instagramAccountResult.IsFailure)
        {
            this._logger.LogError(
                "Failed to retrieve Instagram user info for account {AccountId}: {ErrorMessage}",
                account.Id, instagramAccountResult.Error.Description
            );
            return;
        }

        Result<InstagramAccount> instagramAccount =
            instagramAccountResult.Value.CreateInstagramAccount(new UserId(userId));

        if (instagramAccount.IsFailure)
        {
            this._logger.LogError(
                "Failed to create Instagram account from user info for account {AccountId}: {ErrorMessage}",
                account.Id, instagramAccount.Error.Description
            );
            return;
        }

        InstagramAccount latestAccount = instagramAccount.Value;

        const string updateSql =
            """
            UPDATE analytics.instagram_accounts
            SET
                metadata_id = @MetadataId,
                metadata_ig_id = @MetadataIgId,
                metadata_user_name = @MetadataUserName,
                metadata_followers_count = @MetadataFollowersCount,
                metadata_media_count = @MetadataMediaCount,
                last_updated_at_utc = @LastUpdatedAtUtc
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            updateSql,
            new
            {
                MetadataId = latestAccount.Metadata.Id,
                MetadataIgId = latestAccount.Metadata.IgId,
                MetadataUserName = latestAccount.Metadata.UserName,
                MetadataFollowersCount = latestAccount.Metadata.FollowersCount,
                MetadataMediaCount = latestAccount.Metadata.MediaCount,
                LastUpdatedAtUtc = this._dateTimeProvider.UtcNow,
                latestAccount.Id
            },
            transaction: transaction
        );

        this._logger.LogInformation("Updated metadata for Instagram account {AccountId}", account.Id);
    }
}
