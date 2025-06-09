using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Accounts;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class FacebookService : IFacebookService
{
     private readonly IFacebookApi _facebookApi;
    private readonly ILogger<FacebookService> _logger;

    public FacebookService(IFacebookApi facebookApi, ILogger<FacebookService> logger)
    {
        this._facebookApi = facebookApi;
        this._logger = logger;
    }

    public async Task<Result<string>> GetFacebookPageIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Attempting to get Facebook page ID.");

        FacebookUserInfo? userInfo = await this._facebookApi.GetUserInfoAsync(accessToken, cancellationToken);
        if (userInfo == null)
        {
            this._logger.LogWarning("Failed to retrieve Facebook user info.");
            return Result.Failure<string>(InstagramAccountErrors.FailedToGetFacebookPage);
        }

        FacebookAccountsResponse? accountsData =
            await this._facebookApi.GetAccountsAsync(userInfo.Id, accessToken, cancellationToken);

        if (accountsData?.Data.Length == 1)
        {
            this._logger.LogInformation("Successfully retrieved Facebook page ID.");
            return accountsData.Data[0].Id;
        }

        this._logger.LogWarning("Incorrect number of Facebook pages found.");
        return Result.Failure<string>(InstagramAccountErrors.IncorrectFacebookPagesCount);
    }

    public async Task<Result<string>> GetAdAccountIdAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Attempting to get Advertisement Account ID.");

        FacebookUserInfo? userInfo = await this._facebookApi.GetUserInfoAsync(accessToken, cancellationToken);
        if (userInfo == null)
        {
            this._logger.LogWarning("Failed to retrieve Facebook user info.");
            return Result.Failure<string>(InstagramAccountErrors.FailedToGetFacebookPage);
        }

        JsonElement response =
            await this._facebookApi.GetAdAccountsAsync(userInfo.Id, accessToken, cancellationToken);

        string? adAccountId = response
            .GetProperty("data")
            .EnumerateArray()
            .LastOrDefault()
            .GetProperty("id")
            .GetString();

        if (string.IsNullOrWhiteSpace(adAccountId))
        {
            this._logger.LogWarning("Failed to retrieve ad account ID.");
            return Result.Failure<string>(InstagramAccountErrors.FailedToGetFacebookPage);
        }

        return adAccountId;
    }

    public async Task<FacebookUserInfo?> GetFacebookUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Attempting to get Facebook user info.");
        return await this._facebookApi.GetUserInfoAsync(accessToken, cancellationToken);
    }
}
