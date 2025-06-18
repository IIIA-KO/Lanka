using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models;

public class FacebookTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }
}
