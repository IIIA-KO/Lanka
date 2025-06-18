using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramTokenTokenService : IInstagramTokenService
{
    private readonly IInstagramTokenApi _instagramTokenApi;
    private readonly InstagramOptions _instagramOptions;
    private readonly ILogger<InstagramTokenTokenService> _logger;

    public InstagramTokenTokenService(
        IInstagramTokenApi instagramTokenApi,
        IOptions<InstagramOptions> instagramOptions,
        ILogger<InstagramTokenTokenService> logger
    )
    {
        this._instagramTokenApi = instagramTokenApi;
        this._instagramOptions = instagramOptions.Value;
        this._logger = logger;
    }

    public async Task<Result<FacebookTokenResponse>> GetAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Getting access token via authorization_code");

        var form = new Dictionary<string, string>
        {
            ["client_id"] = this._instagramOptions.ClientId,
            ["client_secret"] = this._instagramOptions.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = this._instagramOptions.RedirectUri,
            ["code"] = code
        };

        FacebookTokenResponse tokenResponse;

        try
        {
            tokenResponse = await this._instagramTokenApi.GetAccessTokenAsync(form, cancellationToken);
        }
        catch (ApiException exception)
        {
            this._logger.LogWarning(exception, "Access token request failed: {StatusCode}", exception.StatusCode);
            return Result.Failure<FacebookTokenResponse>(Error.NotAuthorized);
        }

        this._logger.LogInformation("Access token obtained successfully");

        return await this.AttachExpiration(tokenResponse, cancellationToken);
    }

    public async Task<Result<FacebookTokenResponse>> RenewAccessTokenAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Renewing access token via authorization_code");

        var form = new Dictionary<string, string>
        {
            ["client_id"] = this._instagramOptions.ClientId,
            ["client_secret"] = this._instagramOptions.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = this._instagramOptions.RenewRedirectUri,
            ["code"] = code
        };

        FacebookTokenResponse tokenResponse;

        try
        {
            tokenResponse = await this._instagramTokenApi.GetAccessTokenAsync(form, cancellationToken);
        }
        catch (ApiException exception)
        {
            this._logger.LogWarning(exception, "Renew token request failed: {StatusCode}", exception.StatusCode);
            return Result.Failure<FacebookTokenResponse>(Error.NotAuthorized);
        }

        this._logger.LogInformation("Token renewed successfully");

        return await this.AttachExpiration(tokenResponse, cancellationToken);
    }

    private async Task<Result<FacebookTokenResponse>> AttachExpiration(
        FacebookTokenResponse token,
        CancellationToken cancellationToken
    )
    {
        this._logger.LogInformation("Fetching data_access_expires_at");

        DebugTokenResponse debug;

        try
        {
            debug = await this._instagramTokenApi.GetDebugTokenAsync(
                token.AccessToken,
                token.AccessToken,
                cancellationToken
            );
        }
        catch (ApiException exception)
        {
            this._logger.LogWarning(exception, "Debug token request failed: {StatusCode}", exception.StatusCode);
            return Result.Failure<FacebookTokenResponse>(InstagramAccountErrors.FailedToGetExpirationForAccessToken);
        }

        if (debug?.Data == null)
        {
            this._logger.LogWarning("debug_token response missing data_access_expires_at");
            return Result.Failure<FacebookTokenResponse>(InstagramAccountErrors.FailedToGetExpirationForAccessToken);
        }

        token.ExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(debug.Data.DataAccessExpiresAt);
        this._logger.LogInformation("Expiration attached: {ExpiresAt}", token.ExpiresAtUtc);

        return token;
    }
}
