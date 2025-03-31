using System.Text.Json;
using Lanka.Common.Application.Caching;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Users.Infrastructure.Identity.Services;

internal sealed class KeycloakTokenService : IKeycloakTokenService
{
    private const string CacheKeyPrefix = "jwt:user-";

    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;
    private readonly ICacheService _cacheService;
    private readonly ILogger<KeycloakTokenService> _logger;

    public KeycloakTokenService(
        HttpClient httpClient,
        IOptions<KeycloakOptions> keycloakOptions,
        ICacheService cacheService,
        ILogger<KeycloakTokenService> logger)
    {
        this._httpClient = httpClient;
        this._options = keycloakOptions.Value;
        this._cacheService = cacheService;
        this._logger = logger;
    }

    public async Task<Result<AccessTokenResponse>> GetAccessTokenAsync(
        Email email,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Getting access token for {Email}", email.Value);

        CachedAuthorizationToken? cachedToken = await this.GetCachedAuthorizationTokenAsync(
            email.Value,
            cancellationToken
        );

        if (cachedToken is not null)
        {
            this._logger.LogInformation("Found cached token for {Email}", email);

            return new AccessTokenResponse(
                cachedToken.AccessToken,
                CalculateRemainingTime(cachedToken),
                cachedToken.RefreshToken,
                cachedToken.RefreshExpiresIn
            );
        }

        KeyValuePair<string, string>[] parameters = this.BuildGrantParameters(
            "password",
            new Dictionary<string, string>
            {
                ["username"] = email.Value,
                ["password"] = password
            }
        );

        AuthorizationToken tokenResult = await this.RequestTokenInternalAsync(parameters, cancellationToken);

        if (string.IsNullOrEmpty(tokenResult.AccessToken))
        {
            return Result.Failure<AccessTokenResponse>(IdentityProviderErrors.AuthenticationFailed);
        }

        await this.CacheTokenAsync(email.Value, tokenResult, cancellationToken);
        this._logger.LogInformation("Access token retrieved for {Email}", email);

        return new AccessTokenResponse(
            tokenResult.AccessToken,
            tokenResult.ExpiresIn,
            tokenResult.RefreshToken,
            tokenResult.RefreshExpiresIn
        );
    }

    public async Task<Result<AccessTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Refreshing access token");

        KeyValuePair<string, string>[] parameters = this.BuildGrantParameters("refresh_token",
            new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken
            });

        AuthorizationToken tokenResult = await this.RequestTokenInternalAsync(parameters, cancellationToken);

        if (string.IsNullOrEmpty(tokenResult.AccessToken))
        {
            return Result.Failure<AccessTokenResponse>(IdentityProviderErrors.AuthenticationFailed);
        }

        this._logger.LogInformation("Token refreshed successfully");

        return new AccessTokenResponse(
            tokenResult.AccessToken,
            tokenResult.ExpiresIn,
            tokenResult.RefreshToken,
            tokenResult.RefreshExpiresIn
        );
    }

    #region Private Helpers

    private Dictionary<string, string> GetCommonGrantParameters()
    {
        return new Dictionary<string, string>
        {
            ["client_id"] = this._options.PublicClientId,
            ["client_secret"] = this._options.ConfidentialClientSecret,
            ["scope"] = "openid email"
        };
    }

    private KeyValuePair<string, string>[] BuildGrantParameters(
        string grantType,
        Dictionary<string, string> extraParams
    )
    {
        Dictionary<string, string> parameters = this.GetCommonGrantParameters();
        parameters["grant_type"] = grantType;

        foreach (KeyValuePair<string, string> param in extraParams)
        {
            parameters[param.Key] = param.Value;
        }

        return parameters.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value)).ToArray();
    }

    private async Task<AuthorizationToken> RequestTokenInternalAsync(
        KeyValuePair<string, string>[] parameters,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(parameters);
        using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty);
        request.Content = content;

        try
        {
            HttpResponseMessage response = await this._httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            AuthorizationToken? token = JsonSerializer.Deserialize<AuthorizationToken>(json);
            return token ?? throw new InvalidOperationException("Token response is null.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task CacheTokenAsync(string email, AuthorizationToken tokenResult,
        CancellationToken cancellationToken)
    {
        var cachedToken = new CachedAuthorizationToken(
            tokenResult.AccessToken,
            tokenResult.ExpiresIn,
            tokenResult.RefreshToken,
            tokenResult.RefreshExpiresIn,
            DateTimeOffset.UtcNow
        );

        await this._cacheService.SetAsync(
            $"{CacheKeyPrefix}{email}",
            cachedToken,
            TimeSpan.FromSeconds(tokenResult.ExpiresIn),
            cancellationToken
        );
    }

    private async Task<CachedAuthorizationToken?> GetCachedAuthorizationTokenAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return await this._cacheService
            .GetAsync<CachedAuthorizationToken>(
                $"{CacheKeyPrefix}{email}"
                , cancellationToken
            );
    }

    private static int CalculateRemainingTime(CachedAuthorizationToken cachedToken)
    {
        return (int)(cachedToken.AcquiredAt.AddSeconds(cachedToken.ExpiresIn) - DateTimeOffset.UtcNow).TotalSeconds;
    }

    #endregion
}
