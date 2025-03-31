using System.Text.Json.Serialization;

namespace Lanka.Modules.Users.Infrastructure.Identity.Models;

internal sealed record FederatedIdentity
{
    [JsonPropertyName("identityProvider")] public string IdentityProvider { get; init; }

    [JsonPropertyName("userId")] public string UserId { get; init; }

    [JsonPropertyName("userName")] public string UserName { get; init; }
}
