using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Accounts;
using Microsoft.Extensions.Logging;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramAccountsService : IInstagramAccountsService
{
    private readonly IInstagramAccountsApi _instagramAccountsApi;
    private readonly IFacebookService _facebookService;
    private readonly ILogger<InstagramAccountsService> _logger;

    public InstagramAccountsService(
        IInstagramAccountsApi instagramAccountsApi,
        IFacebookService facebookService,
        ILogger<InstagramAccountsService> logger
    )
    {
        this._instagramAccountsApi = instagramAccountsApi;
        this._facebookService = facebookService;
        this._logger = logger;
    }

    public async Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        string facebookPageId,
        string instagramUsername,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Retrieving Instagram user info…");

        // 1. Resolve Ad Account ID
        Result<string> adAccountIdResult = await this._facebookService.GetAdAccountIdAsync(accessToken, cancellationToken);

        if (adAccountIdResult.IsFailure)
        {
            return this.LogAndReturnFailure<InstagramUserInfo>(adAccountIdResult.Error);
        }

        // 2. Get the Business Account
        var businessQuery = new Dictionary<string, string>
        {
            ["fields"] = "instagram_business_account{id,username}",
            ["access_token"] = accessToken
        };

        InstagramBusinessAccountResponse business;

        try
        {
            business = await this._instagramAccountsApi.GetBusinessAccountAsync(facebookPageId, businessQuery,
                cancellationToken);
        }
        catch (ApiException exception)
        {
            this._logger.LogWarning(exception, "Error fetching business account: {Status}", exception.StatusCode);
            return this.LogAndReturnFailure<InstagramUserInfo>(InstagramAccountErrors.Unexpected);
        }

        if (business?.InstagramAccount == null)
        {
            return Result.Failure<InstagramUserInfo>(Error.NullValue);
        }

        // 3. Determine username
        if (string.IsNullOrWhiteSpace(instagramUsername))
        {
            instagramUsername = business.InstagramAccount.UserName;
        }

        // 4. Fetch full user info via business_discovery
        var userQuery = new Dictionary<string, string>
        {
            ["fields"] = 
                $"business_discovery.username({instagramUsername}){{username,name,ig_id,id,profile_picture_url,biography,followers_count,media_count}}",
            ["access_token"] = accessToken
        };

        InstagramUserInfo userInfo;

        try
        {
            userInfo = await this._instagramAccountsApi.GetUserInfoAsync(business.InstagramAccount.Id, userQuery, cancellationToken);
        }
        catch (ApiException exception)
        {
            this._logger.LogWarning(exception, "Error fetching user info: {Status}", exception.StatusCode);
            return this.LogAndReturnFailure<InstagramUserInfo>(InstagramAccountErrors.Unexpected);
        }

        if (userInfo is not null)
        {
            userInfo.FacebookPageId = facebookPageId;
            userInfo.AdAccountId = adAccountIdResult.Value;
            this._logger.LogInformation("Retrieved Instagram info for {Username}", userInfo.BusinessDiscovery.Username);
        }

        return userInfo;
    }

    public async Task<Result<InstagramUserInfo>> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        // Auto‐resolve the FB page, then delegate
        Result<string> pageIdResult = await this._facebookService.GetFacebookPageIdAsync(accessToken, cancellationToken);
        if (pageIdResult.IsFailure)
        {
            return this.LogAndReturnFailure<InstagramUserInfo>(pageIdResult.Error);
        }

        return await this.GetUserInfoAsync(accessToken, pageIdResult.Value, string.Empty, cancellationToken);
    }

    private Result<T> LogAndReturnFailure<T>(Error error)
    {
        this._logger.LogWarning("Operation failed: {Error}", error);
        return Result.Failure<T>(error);
    }
}
