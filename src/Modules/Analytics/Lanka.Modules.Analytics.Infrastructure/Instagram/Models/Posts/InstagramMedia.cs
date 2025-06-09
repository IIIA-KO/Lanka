using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Posts;

internal sealed class InstagramMedia
{
    [JsonPropertyName("data")] public List<InstagramPostResponse> Data { get; init; }

    [JsonPropertyName("paging")] public InstagramPaging Paging { get; init; }
}
