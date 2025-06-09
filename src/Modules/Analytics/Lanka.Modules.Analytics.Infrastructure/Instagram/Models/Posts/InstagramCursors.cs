using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Posts;

internal sealed class InstagramCursors
{
    [JsonPropertyName("before")] public string Before { get; set; }

    [JsonPropertyName("after")] public string After { get; set; }
}
