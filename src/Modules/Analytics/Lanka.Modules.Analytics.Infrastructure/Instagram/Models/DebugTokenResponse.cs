using System.Text.Json.Serialization;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Models;

public class DebugTokenResponse
{
    public DebugData Data { get; set; } = default!;
}

public class DebugData
{
    [JsonPropertyName("data_access_expires_at")]
    public long DataAccessExpiresAt { get; set; }
}
