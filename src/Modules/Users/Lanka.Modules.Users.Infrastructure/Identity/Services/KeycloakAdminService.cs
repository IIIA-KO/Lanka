using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Infrastructure.Identity.Interfaces;
using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Microsoft.Extensions.Logging;

namespace Lanka.Modules.Users.Infrastructure.Identity.Services;

internal sealed class KeycloakAdminService : IKeycloakAdminService
{
    private const string PasswordCredentialType = "Password";

    private readonly IKeycloakAdminApi _api;
    private readonly ILogger<KeycloakAdminService> _logger;

    public KeycloakAdminService(
        IKeycloakAdminApi api,
        ILogger<KeycloakAdminService> logger)
    {
        this._api = api;
        this._logger = logger;
    }

    public async Task<string> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default
    )
    {
        var representation = new UserRepresentation(
            user.Email,
            user.Email,
            user.FirstName,
            user.LastName,
            EmailVerified: true,
            Enabled: true,
            Credentials:
            [
                new CredentialRepresentation(
                    PasswordCredentialType,
                    user.Password,
                    Temporary: false
                )
            ]
        );
        
        HttpResponseMessage response = await this._api
            .CreateUserAsync(representation);
        response.EnsureSuccessStatusCode();


        return ExtractIdentityIdFromLocationHeader(response);
    }

    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";

        string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery
                                 ?? throw new InvalidOperationException("Location header is null");

        int userSegmentValueIndex = locationHeader.IndexOf(
            usersSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        string identityId = locationHeader.Substring(userSegmentValueIndex + usersSegmentName.Length);

        return identityId;
    }

    public async Task<Result> TerminateUserSession(
        string userIdentityId,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Terminating session for user {UserId}", userIdentityId);

        HttpResponseMessage response = await this._api
            .LogoutUserAsync(userIdentityId);

        if (!response.IsSuccessStatusCode)
        {
            this._logger.LogWarning(
                "Failed to logout user {UserId}. Status code: {StatusCode}",
                userIdentityId, response.StatusCode
            );
            return Result.Failure(IdentityProviderErrors.FailedToTerminateSession);
        }

        this._logger.LogInformation("User {UserId} logged out successfully", userIdentityId);
        return Result.Success();
    }

    public async Task<Result> DeleteAccountAsync(
        string userIdentityId,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Deleting account for user {UserId}", userIdentityId);

        HttpResponseMessage response = await this._api
            .DeleteUserAsync(userIdentityId);

        if (!response.IsSuccessStatusCode)
        {
            this._logger.LogError("Failed to delete account for user {UserId}", userIdentityId);
            return Result.Failure(IdentityProviderErrors.FailedToDeleteAccount);
        }

        this._logger.LogInformation("User {UserId} deleted successfully", userIdentityId);
        return Result.Success();
    }

    public async Task<Result> LinkExternalIdentityProviderAccountToKeycloakUserAsync(
        string userIdentityId,
        string providerName,
        string providerUserId,
        string providerUsername,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation(
            "Linking external provider {ProviderName} to user {UserId}",
            providerName, userIdentityId
        );

        var payload = new FederatedIdentity
        {
            IdentityProvider = providerName,
            UserId = providerUserId,
            UserName = providerUsername
        };

        HttpResponseMessage response = await this._api
            .LinkProviderAsync(userIdentityId, providerName, payload);

        if (!response.IsSuccessStatusCode)
        {
            this._logger.LogWarning(
                "Failed to link {ProviderName} to user {UserId}",
                providerName, userIdentityId
            );
            return Result.Failure(IdentityProviderErrors.InvalidCredentials);
        }

        this._logger.LogInformation(
            "Successfully linked {ProviderName} to user {UserId}",
            providerName, userIdentityId
        );
        return Result.Success();
    }

    public async Task<bool> IsExternalIdentityProviderAccountLinkedAsync(
        string userIdentityId,
        string providerName,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation(
            "Checking external provider {ProviderName} for user {UserId}",
            providerName, userIdentityId
        );

        List<FederatedIdentity> identities = await this._api
            .GetFederatedIdentitiesAsync(userIdentityId);

        bool isLinked = identities
            .Any(f => f.IdentityProvider.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        this._logger.LogInformation(
            "Provider {ProviderName} is {Status} for user {UserId}",
            providerName,
            isLinked ? "linked" : "not linked",
            userIdentityId
        );

        return isLinked;
    }

    public async Task<bool> CheckUserExistsInKeycloak(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        List<UserRepresentation> users = await this._api
            .QueryUsersByEmailAsync(email);

        return users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
