namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;

public sealed class InstagramPost
{
    public string Id { get; set; }

    public string MediaType { get; set; }

    public string MediaUrl { get; set; }

    public string Permalink { get; set; }

    public string ThumbnailUrl { get; set; }

    public DateTime Timestamp { get; set; }

    public List<InstagramInsight> Insights { get; set; }
}
