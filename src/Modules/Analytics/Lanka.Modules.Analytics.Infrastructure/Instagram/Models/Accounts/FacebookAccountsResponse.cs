using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Accounts;

internal sealed class FacebookAccountsResponse
{
    [JsonPropertyName("data")] public FacebookPage[] Data { get; set; }
}

public class FacebookPage
{
    [JsonPropertyName("id")]  public string Id { get; set; }
}
