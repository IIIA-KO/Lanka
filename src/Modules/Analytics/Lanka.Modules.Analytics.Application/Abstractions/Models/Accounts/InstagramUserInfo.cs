using System.Text.Json.Serialization;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;

public sealed class InstagramUserInfo
{
    [JsonPropertyName("business_discovery")]
    public BusinessDiscovery BusinessDiscovery { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    public string FacebookPageId { get; set; }

    public string AdAccountId { get; set; }

    public Result<InstagramAccount> CreateInstagramAccount(Guid userId, string email)
    {
        return InstagramAccount.Create(
            userId,
            email,
            this.FacebookPageId,
            this.AdAccountId,
            this.BusinessDiscovery.Id,
            this.BusinessDiscovery.IgId,
            this.BusinessDiscovery.Username,
            this.BusinessDiscovery.FollowersCount,
            this.BusinessDiscovery.MediaCount
        );
    }
}

public class BusinessDiscovery
{
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ig_id")] public long IgId { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("followers_count")] public int FollowersCount { get; set; }

    [JsonPropertyName("media_count")] public int MediaCount { get; set; }
}
