using Lanka.Common.Application.Caching;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Infrastructure.Identity.Interfaces;
using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Users.Infrastructure.Identity.Services;

internal sealed class KeycloakTokenService : IKeycloakTokenService
{
    private const string _cacheKeyPrefix = "jwt:user-";

    private readonly IKeycloakTokenApi _api;
    private readonly KeycloakOptions _options;
    private readonly ICacheService _cacheService;
    private readonly ILogger<KeycloakTokenService> _logger;

    public KeycloakTokenService(
        IKeycloakTokenApi api,
        IOptions<KeycloakOptions> keycloakOptions,
        ICacheService cacheService,
        ILogger<KeycloakTokenService> logger)
    {
        this._api = api;
        this._options = keycloakOptions.Value;
        this._cacheService = cacheService;
        this._logger = logger;
    }

    public async Task<Result<AccessTokenResponse>> GetAccessTokenAsync(
        Email email,
        string password,
        CancellationToken cancellationToken = default)
    {
        CachedAuthorizationToken? cached = await this.GetCachedAuthorizationTokenAsync(
            email.Value,
            cancellationToken
        );
        
        if (cached is not null)
        {
            return new AccessTokenResponse(
                cached.AccessToken,
                ExpiresIn: CalculateRemainingTime(cached),
                cached.RefreshToken,
                cached.RefreshExpiresIn
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
        
        using var content = new FormUrlEncodedContent(parameters);
        AuthorizationToken token = await this._api.GetTokenAsync(content);

        if (string.IsNullOrEmpty(token.AccessToken))
        {
            return Result.Failure<AccessTokenResponse>(IdentityProviderErrors.AuthenticationFailed);
        }

        await this.CacheTokenAsync(email.Value, token, cancellationToken);

        return new AccessTokenResponse(
            token.AccessToken,
            token.ExpiresIn,
            token.RefreshToken,
            token.RefreshExpiresIn
        );
    }

    public async Task<Result<AccessTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        this._logger.LogInformation("Refreshing access token");

        KeyValuePair<string, string>[] parameters = this.BuildGrantParameters(
            "refresh_token",
            new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken
            }
        );

        AuthorizationToken token = await this.RequestTokenInternalAsync(parameters);

        if (string.IsNullOrEmpty(token.AccessToken))
        {
            return Result.Failure<AccessTokenResponse>(IdentityProviderErrors.AuthenticationFailed);
        }

        this._logger.LogInformation("Token refreshed successfully");
        return new AccessTokenResponse(
            token.AccessToken,
            token.ExpiresIn,
            token.RefreshToken,
            token.RefreshExpiresIn
        );
    }
    

    #region Private Helpers
    
    private async Task<AuthorizationToken> RequestTokenInternalAsync(KeyValuePair<string, string>[] parameters)
    {
        using var content = new FormUrlEncodedContent(parameters);
        AuthorizationToken token = await this._api.GetTokenAsync(content);
        return token;
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

    private Dictionary<string, string> GetCommonGrantParameters()
    {
        return new Dictionary<string, string>
        {
            ["client_id"] = this._options.PublicClientId,
            ["client_secret"] = this._options.ConfidentialClientSecret,
            ["scope"] = "openid email"
        };
    }

    private async Task<CachedAuthorizationToken?> GetCachedAuthorizationTokenAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return await this._cacheService
            .GetAsync<CachedAuthorizationToken>(
                $"{_cacheKeyPrefix}{email}",
                cancellationToken
            );
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
            $"{_cacheKeyPrefix}{email}",
            cachedToken,
            TimeSpan.FromSeconds(tokenResult.ExpiresIn),
            cancellationToken
        );
    }

    private static int CalculateRemainingTime(CachedAuthorizationToken cachedToken)
    {
        return (int)(cachedToken.AcquiredAt.AddSeconds(cachedToken.ExpiresIn) - DateTimeOffset.UtcNow).TotalSeconds;
    }

    #endregion
}
