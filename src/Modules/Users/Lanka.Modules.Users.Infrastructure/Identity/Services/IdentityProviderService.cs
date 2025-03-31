using System.Net;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Application.Users.Login;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Infrastructure.Identity.Services;

internal sealed class IdentityProviderService : IIdentityProviderService
{
    private readonly KeycloakTokenService _keycloakTokenService;
    private readonly KeycloakAdminService _keycloakAdminService;
    private readonly ILogger<IdentityProviderService> _logger;

    public IdentityProviderService(
        KeycloakTokenService keycloakTokenService,
        KeycloakAdminService keycloakAdminService,
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
        this._logger.LogInformation(
            "Attempting to get access token for email: {Email}",
            email
        );
        
        Result<AccessTokenResponse> tokenResponse = await this._keycloakTokenService.GetAccessTokenAsync(
            email,
            password,
            cancellationToken
        );
        
        if (tokenResponse.IsFailure)
        {
            return Result.Failure<AccessTokenResponse>(IdentityProviderErrors.EmailIsNotUnique);
        }
        return tokenResponse;
    }
}
