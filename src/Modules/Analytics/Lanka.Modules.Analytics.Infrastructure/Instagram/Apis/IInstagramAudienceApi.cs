using System.Text.Json;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

internal interface IInstagramAudienceApi
{
    [Get("/{instagramAccountId}/insights")]
    Task<JsonElement> GetAudienceInsightsAsync(
        string instagramAccountId,
        [Query] string metric,
        [Query] string period,
        [AliasAs("metric_type")] string metricType,
        [Query] string breakdown,
        [AliasAs("access_token")] string accessToken,
        CancellationToken cancellationToken = default
    );

    [Get("/{instagramAccountId}/insights")]
    Task<JsonElement> GetReachInsightsAsync(
        string instagramAccountId,
        [Query] string metric,
        [Query] string period,
        [Query] string since,
        [Query] string until,
        [AliasAs("breakdown")] string breakdown,
        [AliasAs("metric_type")] string metricType,
        [AliasAs("access_token")] string accessToken,
        CancellationToken cancellationToken = default
    );
}
