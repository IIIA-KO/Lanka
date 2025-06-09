using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Models.Posts;

internal sealed class InstagramPaging
{
    [JsonPropertyName("cursors")] public InstagramCursors Cursors { get; set; }

    [JsonPropertyName("next")] public string Next { get; set; }

    [JsonPropertyName("previous")] public string Previous { get; set; }
}
