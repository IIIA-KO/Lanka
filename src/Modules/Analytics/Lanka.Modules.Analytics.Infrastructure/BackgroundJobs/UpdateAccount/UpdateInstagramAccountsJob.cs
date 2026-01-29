using System.Threading.RateLimiting;
using Lanka.Common.Application.Clock;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Metadatas;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.UpdateAccount;

[DisallowConcurrentExecution]
internal sealed class UpdateInstagramAccountsJob : IJob
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly InstagramOptions _instagramOptions;
    private readonly IInstagramServiceFactory<IInstagramAccountsService> _instagramAccountsServiceFactory;
    private readonly RateLimiter _rateLimiter;
    private readonly IInstagramAccountRepository _instagramAccountRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateInstagramAccountsJob> _logger;

    public UpdateInstagramAccountsJob(
        IDateTimeProvider dateTimeProvider,
        IOptions<InstagramOptions> instagramOptions,
        IInstagramServiceFactory<IInstagramAccountsService> instagramAccountsServiceFactory,
        RateLimiter rateLimiter,
        IInstagramAccountRepository instagramAccountRepository,
        ITokenRepository tokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateInstagramAccountsJob> logger
    )
    {
        this._dateTimeProvider = dateTimeProvider;
        this._instagramOptions = instagramOptions.Value;
        this._instagramAccountsServiceFactory = instagramAccountsServiceFactory;
        this._rateLimiter = rateLimiter;
        this._instagramAccountRepository = instagramAccountRepository;
        this._tokenRepository = tokenRepository;
        this._unitOfWork = unitOfWork;
        this._logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        this._logger.LogInformation("Beginning UpdateInstagramAccountsJob execution");

        CancellationToken cancellationToken = context.CancellationToken;

        InstagramAccount[] instagramAccounts = await this._instagramAccountRepository.GetOldAccountsAsync(
            this._instagramOptions.RenewalThresholdInDays,
            this._instagramOptions.BatchSize,
            cancellationToken
        );

        int successCount = 0;
        int failureCount = 0;
        int rateLimitedCount = 0;

        foreach (InstagramAccount account in instagramAccounts)
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
                await this.UpdateInstagramAccountMetadataAsync(account, cancellationToken);

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
                return;
            }
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        this._logger.LogInformation("Completed UpdateInstagramAccountsJob execution");
    }

    private async Task UpdateInstagramAccountMetadataAsync(
        InstagramAccount account,
        CancellationToken cancellationToken
    )
    {
        this._logger.LogInformation(
            "Updating metadata for Instagram account {AccountId} at {Time}",
            account.Id.Value, this._dateTimeProvider.UtcNow
        );

        Token? token = await this._tokenRepository.GetByUserIdAsync(
            account.UserId,
            cancellationToken
        );

        if (token is null)
        {
            this._logger.LogWarning(
                "No token found for user associated with Instagram account {AccountId}",
                account.Id
            );

            return;
        }

        // Skip fake/mock accounts created during seeding (they have placeholder tokens)
        if (token.AccessToken.Value.StartsWith("fake_token_", StringComparison.OrdinalIgnoreCase))
        {
            this._logger.LogDebug(
                "Skipping mock Instagram account {AccountId} with fake token",
                account.Id
            );

            return;
        }

        // Get the appropriate service based on user's email (real for allowed users, mock for others in Development)
        IInstagramAccountsService instagramAccountsService = this._instagramAccountsServiceFactory
            .GetService(account.Email.Value);

        Result<InstagramUserInfo> instagramAccountResult = await instagramAccountsService
            .GetUserInfoAsync(
                token.AccessToken.Value,
                account.FacebookPageId.Value,
                account.Metadata.UserName,
                cancellationToken
            );

        if (instagramAccountResult.IsFailure)
        {
            this._logger.LogError(
                "Failed to retrieve Instagram user info for account {AccountId}: {ErrorMessage}",
                account.Id, instagramAccountResult.Error.Description
            );
            return;
        }

        InstagramUserInfo fetchedInstagramAccount = instagramAccountResult.Value;

        InstagramAccount? existingInstagramAccount = await this._instagramAccountRepository.GetByUserIdAsync(
            account.UserId,
            cancellationToken
        );

        if (existingInstagramAccount is null)
        {
            return;
        }

        existingInstagramAccount.Update(
            Metadata.Create(
                fetchedInstagramAccount.BusinessDiscovery.Id,
                fetchedInstagramAccount.BusinessDiscovery.IgId,
                fetchedInstagramAccount.BusinessDiscovery.Username,
                fetchedInstagramAccount.BusinessDiscovery.FollowersCount,
                fetchedInstagramAccount.BusinessDiscovery.MediaCount
            ).Value
        );

        this._logger.LogInformation("Updated metadata for Instagram account {AccountId}", account.Id);
    }
}
