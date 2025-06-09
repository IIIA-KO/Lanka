using System.Text.Json;
using Refit;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

internal interface IInstagramStatisticsApi
{
    [Get("/{accountId}/insights")]
    Task<JsonElement> GetInsightsAsync(
        [AliasAs("accountId")] string accountId,
        [Query] string metric,
        [Query] string period,
        [Query] string since,
        [Query] string until,
        [Query] string access_token,
        [Query] string? metric_type = null,
        CancellationToken cancellationToken = default
    );

    [Get("/{adAccountId}/insights")]
    Task<JsonElement> GetAdInsightsAsync(
        [AliasAs("adAccountId")] string adAccountId,
        [Query] string fields,
        [Query] string since,
        [Query] string until,
        [Query] string access_token,
        CancellationToken cancellationToken = default
    );
}
