using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Users.Infrastructure.Identity;

internal sealed class KeycloakAuthDelegatingHandler : DelegatingHandler
{
    private readonly KeycloakOptions _options;

    public KeycloakAuthDelegatingHandler(IOptions<KeycloakOptions> options)
    {
        this._options = options.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        AuthToken authorizationToken = await this.GetAuthorizationToken(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken.AccessToken);

        HttpResponseMessage httpResponseMessage = await base.SendAsync(request, cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();

        return httpResponseMessage;
    }

    private async Task<AuthToken> GetAuthorizationToken(CancellationToken cancellationToken)
    {
        var authRequestParameters = new KeyValuePair<string, string>[]
        {
            new("client_id", this._options.ConfidentialClientId),
            new("client_secret", this._options.ConfidentialClientSecret),
            new("scope", "openid"),
            new("grant_type", "client_credentials")
        };

        using var authRequestContent = new FormUrlEncodedContent(authRequestParameters);

        using var authRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(this._options.TokenUrl));
        authRequest.Content = authRequestContent;

        using HttpResponseMessage authorizationResponse = await base.SendAsync(authRequest, cancellationToken);
        authorizationResponse.EnsureSuccessStatusCode();

        return await authorizationResponse.Content.ReadFromJsonAsync<AuthToken>(cancellationToken);
    }

    internal sealed class AuthToken
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; }
    }
}
