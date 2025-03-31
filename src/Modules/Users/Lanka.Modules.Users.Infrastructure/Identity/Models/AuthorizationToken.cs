using System.Text.Json.Serialization;

namespace Lanka.Modules.Users.Infrastructure.Identity.Models;

internal sealed class AuthorizationToken
{
    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; init; } = string.Empty;
    
    [JsonPropertyName("refresh_expires_in")] public int RefreshExpiresIn { get; set; }
}
