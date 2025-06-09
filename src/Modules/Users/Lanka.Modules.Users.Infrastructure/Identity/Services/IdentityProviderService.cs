using System.Net;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users.Emails;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Infrastructure.Identity.Services;

internal sealed class IdentityProviderService : IIdentityProviderService
{
    private readonly IKeycloakTokenService _keycloakTokenService;
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly ILogger<IdentityProviderService> _logger;

    public IdentityProviderService(
        IKeycloakTokenService keycloakTokenService,
        IKeycloakAdminService keycloakAdminService,
        ILogger<IdentityProviderService> logger
    )
    {
        this._keycloakTokenService = keycloakTokenService;
        this._keycloakAdminService = keycloakAdminService;
        this._logger = logger;
    }

    public async Task<Result<string>> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            string identityId = await this._keycloakAdminService.RegisterUserAsync(user, cancellationToken);

            return identityId;
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            this._logger.LogError(exception, "Failed to register user");

            return Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique);
        }
    }

    public async Task<Result<AccessTokenResponse>> GetAccessTokenAsync(
        Email email,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        Result<AccessTokenResponse> tokenResponse = await this._keycloakTokenService.GetAccessTokenAsync(
            email,
            password,
            cancellationToken
        );

        return tokenResponse.IsFailure
            ? Result.Failure<AccessTokenResponse>(IdentityProviderErrors.EmailIsNotUnique)
            : tokenResponse;
    }
    
    public async Task<Result<AccessTokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        Result<AccessTokenResponse> tokenResponse = await this._keycloakTokenService.RefreshTokenAsync(
            refreshToken,
            cancellationToken
        );

        return tokenResponse.IsFailure
            ? Result.Failure<AccessTokenResponse>(tokenResponse.Error)
            : tokenResponse;
    }

    public async Task<Result> TerminateUserSessionAsync(string userIdentityId, CancellationToken cancellationToken = default)
    {
        Result result = await this._keycloakAdminService.TerminateUserSession(
            userIdentityId,
            cancellationToken
        );

        return result;
    }
    
    public async Task<Result> DeleteUserAsync(string userIdentityId, CancellationToken cancellationToken = default)
    {
        Result result = await this._keycloakAdminService.DeleteAccountAsync(
            userIdentityId,
            cancellationToken
        );

        return result;
    }
    
    public async Task<Result> LinkExternalAccountToUserAsync(
        string userIdentityId,
        string providerName,
        string providerUserId,
        string providerUsername,
        CancellationToken cancellationToken = default
    )
    {
        Result result = await this._keycloakAdminService.LinkExternalAccountToUserAsync(
            userIdentityId,
            providerName,
            providerUserId,
            providerUsername,
            cancellationToken
        );

        return result;
    }
    
    public async Task<bool> IsExternalAccountLinkedAsync(
        string userIdentityId,
        string providerName,
        CancellationToken cancellationToken = default
    )
    {
        bool isLinked = await this._keycloakAdminService.IsExternalAccountLinkedAsync(
            userIdentityId,
            providerName,
            cancellationToken
        );

        return isLinked;
    }
    
    public async Task<bool> CheckUserExistsInKeycloak(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        bool exists = await this._keycloakAdminService.CheckUserExistsInKeycloak(
            email,
            cancellationToken
        );

        return exists;
    }
}
