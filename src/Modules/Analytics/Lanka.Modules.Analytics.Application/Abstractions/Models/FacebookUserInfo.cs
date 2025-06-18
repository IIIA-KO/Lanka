using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models;

public sealed class FacebookUserInfo
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }
}
