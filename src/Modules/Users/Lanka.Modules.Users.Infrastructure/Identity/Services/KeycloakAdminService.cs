using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Users.Application.Abstractions.Identity;
using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Users.Infrastructure.Identity.Services;

internal sealed class KeycloakAdminService : IKeycloakAdminService
{
    private const string PasswordCredentialType = "Password";

    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakAdminService> _logger;

    public KeycloakAdminService(
        HttpClient httpClient,
        IOptions<KeycloakOptions> keycloakOptions,
        ILogger<KeycloakAdminService> logger)
    {
        this._httpClient = httpClient;
        this._options = keycloakOptions.Value;
        this._logger = logger;
    }

    public async Task<string> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default
    )
    {
        var userRepresentation = new UserRepresentation(
            user.Email,
            user.Email,
            user.FirstName,
            user.LastName,
            true,
            true,
            [new CredentialRepresentation(PasswordCredentialType, user.Password, false)]);

        HttpResponseMessage httpResponseMessage = await this._httpClient.PostAsJsonAsync(
            "users",
            userRepresentation,
            cancellationToken);

        httpResponseMessage.EnsureSuccessStatusCode();

        return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
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
        string accessToken = await this.GetAdminAccessTokenAsync(cancellationToken);

        string requestUrl =
            $"{this._options.BaseUrl}/admin/realms/{this._options.Realm}/users/{userIdentityId}/logout";

        HttpResponseMessage response = await this.SendAuthorizedRequestAsync(
            HttpMethod.Post,
            requestUrl,
            accessToken,
            null,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            this._logger.LogWarning("Failed to logout user {UserId}. Status code: {StatusCode}", userIdentityId,
                response.StatusCode);
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
        string accessToken = await this.GetAdminAccessTokenAsync(cancellationToken);

        string url =
            $"{this._options.AdminUrl}/users/{userIdentityId}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await this._httpClient.SendAsync(request, cancellationToken);

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
        this._logger.LogInformation("Linking external provider {ProviderName} to user {UserId}", providerName,
            userIdentityId);
        string accessToken = await this.GetAdminAccessTokenAsync(cancellationToken);

        string requestUrl =
            $"{this._options.AdminUrl}/users/{userIdentityId}/federated-identity/{providerName}";

        var payload = new
        {
            identityProvider = providerName,
            userId = providerUserId,
            userName = providerUsername
        };

        HttpResponseMessage response = await this.SendAuthorizedRequestAsync(
            HttpMethod.Post,
            requestUrl,
            accessToken,
            payload,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            this._logger.LogInformation("Successfully linked {ProviderName} to user {UserId}", providerName,
                userIdentityId);
            return Result.Success();
        }
        else
        {
            this._logger.LogWarning("Failed to link {ProviderName} to user {UserId}", providerName, userIdentityId);
            return Result.Failure(IdentityProviderErrors.InvalidCredentials);
        }
    }

    public async Task<bool> IsExternalIdentityProviderAccountLinkedAsync(
        string userIdentityId,
        string providerName,
        CancellationToken cancellationToken = default
    )
    {
        this._logger.LogInformation("Checking external provider {ProviderName} for user {UserId}", providerName,
            userIdentityId);
        string accessToken = await this.GetAdminAccessTokenAsync(cancellationToken);

        string requestUrl =
            $"{this._options.AdminUrl}/users/{userIdentityId}/federated-identity";

        HttpResponseMessage response = await this.SendAuthorizedRequestAsync(
            HttpMethod.Get,
            requestUrl,
            accessToken,
            string.Empty,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            this._logger.LogWarning("Failed to get federated identities for user {UserId}. Status code: {StatusCode}",
                userIdentityId, response.StatusCode);
            return false;
        }

        List<FederatedIdentity>? federatedIdentities = await JsonSerializer.DeserializeAsync<List<FederatedIdentity>>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken
        );

        bool isLinked = federatedIdentities?.Any(f =>
            f.IdentityProvider.Equals(providerName, StringComparison.OrdinalIgnoreCase)
        ) ?? false;

        this._logger.LogInformation("Provider {ProviderName} is {Status} for user {UserId}", providerName,
            isLinked ? "linked" : "not linked", userIdentityId);
        return isLinked;
    }

    public async Task<bool> CheckUserExistsInKeycloak(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        string accessToken = await this.GetAdminAccessTokenAsync(cancellationToken);

        string requestUrl =
            $"{this._options.AdminUrl}/users?email={email}";

        HttpResponseMessage response = await this.SendAuthorizedRequestAsync(
            HttpMethod.Get,
            requestUrl,
            accessToken,
            string.Empty,
            cancellationToken
        );

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        return response.IsSuccessStatusCode && content.Contains(email);
    }

    #region Private Helpers

    private async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken)
    {
        var parameters = new KeyValuePair<string, string>[]
        {
            new("client_id", this._options.ConfidentialClientId),
            new("client_secret", this._options.ConfidentialClientSecret),
            new("grant_type", "client_credentials")
        };

        using var content = new FormUrlEncodedContent(parameters);
        HttpResponseMessage response = await this._httpClient.PostAsync(string.Empty, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        AuthorizationToken? tokenResponse = JsonSerializer.Deserialize<AuthorizationToken>(json);
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Admin token response is null.");
    }

    private async Task<HttpResponseMessage> SendAuthorizedRequestAsync(
        HttpMethod method,
        string url,
        string accessToken,
        object? payload,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (payload is not null)
        {
            string json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await this._httpClient.SendAsync(request, cancellationToken);
    }

    #endregion
}
