using Lanka.Modules.Analytics.Domain.UserActivities;
using MongoDB.Bson.Serialization.Attributes;

namespace Lanka.Modules.Analytics.Domain.Posts;

public sealed class InstagramPosts : AnalyticsDataWithTtl
{
    [BsonId] public Guid InstagramAccountId { get; set; }

    public List<CachedInstagramPost> Posts { get; set; } = [];

    public InstagramPagingInfo Paging { get; set; } = new();

    public InstagramPosts() { }

    public InstagramPosts(UserActivityLevel activityLevel)
        : base(GetTtlForPostsActivityLevel(activityLevel))
    {
    }

    private static TimeSpan GetTtlForPostsActivityLevel(UserActivityLevel activityLevel) => activityLevel switch
    {
        UserActivityLevel.PowerUser => TimeSpan.FromHours(12),
        UserActivityLevel.Active => TimeSpan.FromHours(24),
        UserActivityLevel.Occasional => TimeSpan.FromHours(36),
        UserActivityLevel.Inactive => TimeSpan.FromHours(48),
        _ => TimeSpan.FromHours(24)
    };
}

public sealed class CachedInstagramPost
{
    public string Id { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public string MediaUrl { get; set; } = string.Empty;

    public string Permalink { get; set; } = string.Empty;

    public string ThumbnailUrl { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public List<CachedInsight> Insights { get; set; } = [];
}

public sealed class CachedInsight
{
    public string Name { get; set; } = string.Empty;

    public string Period { get; set; } = string.Empty;

    public List<CachedInsightValue> Values { get; set; } = [];
}

public sealed class CachedInsightValue
{
    public int? Value { get; set; }
}

public sealed class InstagramPagingInfo
{
    public string? After { get; set; }

    public string? Before { get; set; }

    public string? NextCursor { get; set; }

    public string? PreviousCursor { get; set; }
}
