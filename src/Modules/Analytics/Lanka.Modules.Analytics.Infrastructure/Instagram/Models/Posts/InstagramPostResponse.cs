using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Posts;

internal sealed class InstagramPostResponse
{
    [JsonPropertyName("id")] public string Id { get; init; }

    [JsonPropertyName("media_type")] public string MediaType { get; init; }

    [JsonPropertyName("media_url")] public string MediaUrl { get; init; }

    [JsonPropertyName("permalink")] public string Permalink { get; init; }

    [JsonPropertyName("thumbnail_url")] public string ThumbnailUrl { get; init; }

    [JsonPropertyName("timestamp")] public string Timestamp { get; init; }

    public InstagramInsightsResponse? Insights { get; internal set; }
}

internal sealed class InstagramInsightsResponse
{
    [JsonPropertyName("data")] public List<InstagramInsightResponse>? Data { get; init; }
}

internal sealed class InstagramInsightResponse
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("period")] public string Period { get; set; }

    [JsonPropertyName("values")] public List<InsightValueResponse> Values { get; set; }
}

internal sealed class InsightValueResponse
{
    [JsonPropertyName("value")] public int? Value { get; set; }
}
