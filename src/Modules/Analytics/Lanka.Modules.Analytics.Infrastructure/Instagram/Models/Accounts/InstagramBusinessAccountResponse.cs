using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Accounts;

internal sealed class InstagramBusinessAccountResponse
{
    [JsonPropertyName("instagram_business_account")]
    public InstagramAccountResponse? InstagramAccount { get; set; }

    [JsonPropertyName("id")] public string FacebookBusinessPageId { get; set; }
}

internal class InstagramAccountResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("username")] public string UserName { get; set; }
}
