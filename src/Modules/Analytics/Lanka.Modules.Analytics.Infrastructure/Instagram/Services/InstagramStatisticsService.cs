using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramStatisticsService : IInstagramStatisticsService
{
    private readonly IInstagramStatisticsApi _instagramStatisticsApi;

    public InstagramStatisticsService(IInstagramStatisticsApi instagramStatisticsApi)
    {
        this._instagramStatisticsApi = instagramStatisticsApi;
    }

    public async Task<Result<TableStatistics>> GetTableStatistics(
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement response = await this._instagramStatisticsApi.GetInsightsAsync(
            request.InstagramAccountId,
            metric: "reach,follower_count,impressions,profile_views",
            period: "day",
            since: request.Since.ToString(CultureInfo.InvariantCulture),
            until: request.Until.ToString(CultureInfo.InvariantCulture),
            access_token: request.AccessToken,
            cancellationToken: cancellationToken
        );

        return response.ValueKind != JsonValueKind.Undefined
            ? TableStatistics.ParseTableStatistics(response)
            : Result.Failure<TableStatistics>(InstagramAccountErrors.Unexpected);
    }

    public async Task<Result<OverviewStatistics>> GetOverviewStatistics(
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement response = await this._instagramStatisticsApi.GetInsightsAsync(
            request.InstagramAccountId,
            metric: "reach,impressions,total_interactions,comments,profile_views,website_clicks",
            period: "day",
            since: request.Since.ToString(CultureInfo.InvariantCulture),
            until: request.Until.ToString(CultureInfo.InvariantCulture),
            access_token:request.AccessToken,
            metric_type: "total_value",
            cancellationToken: cancellationToken
        );

        return response.ValueKind != JsonValueKind.Undefined
            ? OverviewStatistics.ParseOverviewStatistics(response)
            : Result.Failure<OverviewStatistics>(InstagramAccountErrors.Unexpected);
    }

    public async Task<Result<InteractionStatistics>> GetInteractionsStatistics(
        string instagramAdAccountId,
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement insights = await this._instagramStatisticsApi.GetInsightsAsync(
            request.InstagramAccountId,
            metric: "total_interactions,reach,likes,comments",
            period: "day",
            since: request.Since.ToString(CultureInfo.InvariantCulture),
            until: request.Until.ToString(CultureInfo.InvariantCulture),
            access_token: request.AccessToken,
            metric_type: "total_value",
            cancellationToken: cancellationToken
        );

        JsonElement ads = await this._instagramStatisticsApi.GetAdInsightsAsync(
            instagramAdAccountId,
            fields: "spend",
            since: request.Since.ToString(CultureInfo.InvariantCulture),
            until: request.Until.ToString(CultureInfo.InvariantCulture),
            access_token: request.AccessToken,
            cancellationToken: cancellationToken
        );

        int daysCount = request.Until.DayNumber - request.Since.DayNumber + 1;

        return insights.ValueKind != JsonValueKind.Undefined
            ? InteractionStatistics.ParseInteractionStatistics(insights, ads, daysCount)
            : Result.Failure<InteractionStatistics>(InstagramAccountErrors.Unexpected);
    }

    public async Task<Result<EngagementStatistics>> GetEngagementStatistics(
        int followersCount,
        InstagramPeriodRequest request,
        CancellationToken cancellationToken = default
    )
    {
        JsonElement response = await this._instagramStatisticsApi.GetInsightsAsync(
            request.InstagramAccountId,
            "reach,impressions,profile_views,likes,comments,saves",
            "day",
            request.Since.ToString(CultureInfo.InvariantCulture),
            request.Until.ToString(CultureInfo.InvariantCulture),
            request.AccessToken,
            metric_type: "total_value",
            cancellationToken: cancellationToken
        );

        return response.ValueKind != JsonValueKind.Undefined
            ? EngagementStatistics.ParseEngagementsStatistics(response, followersCount)
            : Result.Failure<EngagementStatistics>(InstagramAccountErrors.Unexpected);
    }
}
