using System.Globalization;
using System.Text.Json;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Instagram.UserActivity;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Domain.Tokens;
using Lanka.Modules.Analytics.Domain.UserActivities;
using Lanka.Modules.Analytics.Infrastructure.Instagram.Apis;
using Lanka.Modules.Analytics.Infrastructure.Statistics;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Services;

internal sealed class InstagramStatisticsService : IInstagramStatisticsService
{
    private readonly IInstagramStatisticsApi _instagramStatisticsApi;
    private readonly EngagementRepository _engagementRepository;
    private readonly InteractionRepository _interactionRepository;
    private readonly MetricsRepository _metricsRepository;
    private readonly OverviewRepository _overviewRepository;
    private readonly IUserActivityService _userActivityService;

    public InstagramStatisticsService(
        IInstagramStatisticsApi instagramStatisticsApi,
        EngagementRepository engagementRepository,
        InteractionRepository interactionRepository,
        MetricsRepository metricsRepository,
        OverviewRepository overviewRepository,
        IUserActivityService userActivityService
    )
    {
        this._instagramStatisticsApi = instagramStatisticsApi;
        this._engagementRepository = engagementRepository;
        this._interactionRepository = interactionRepository;
        this._metricsRepository = metricsRepository;
        this._overviewRepository = overviewRepository;
        this._userActivityService = userActivityService;
    }

    public async Task<Result<MetricsStatistics>> GetMetricsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        MetricsStatistics? validData = await this._metricsRepository.GetValidAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (validData is not null)
        {
            return validData;
        }

        MetricsStatistics? fallbackData = await this._metricsRepository.GetAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<MetricsStatistics>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        var dateRange = new InstagramPeriodCalculator(statisticsPeriod);

        try
        {
            JsonElement response = await this._instagramStatisticsApi.GetInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "reach,follower_count,impressions,profile_views",
                period: "day",
                since: dateRange.Since.ToString(CultureInfo.InvariantCulture),
                until: dateRange.Until.ToString(CultureInfo.InvariantCulture),
                access_token: accessToken,
                cancellationToken: cancellationToken
            );

            if (response.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<MetricsStatistics>(MetricsStatistics.InvalidData);
            }

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<MetricsStatistics> result = MetricsStatistics.Create(response, userActivityLevel);

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            MetricsStatistics metricsStatistics = result.Value;
            metricsStatistics.InstagramAccountId = instagramAccount.Id.Value;
            metricsStatistics.StatisticsPeriod = statisticsPeriod;

            await this._metricsRepository.ReplaceAsync(metricsStatistics, statisticsPeriod, cancellationToken);

            return metricsStatistics;
        }
        catch
        {
            return Result.Failure<MetricsStatistics>(MetricsStatistics.InvalidData);
        }
    }

    public async Task<Result<OverviewStatistics>> GetOverviewStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        OverviewStatistics? validData = await this._overviewRepository.GetValidAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (validData is not null)
        {
            return validData;
        }

        OverviewStatistics? fallbackData = await this._overviewRepository.GetAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<OverviewStatistics>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        var dateRange = new InstagramPeriodCalculator(statisticsPeriod);

        try
        {
            JsonElement response = await this._instagramStatisticsApi.GetInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "reach,impressions,total_interactions,comments,profile_views,website_clicks",
                period: "day",
                since: dateRange.Since.ToString(CultureInfo.InvariantCulture),
                until: dateRange.Until.ToString(CultureInfo.InvariantCulture),
                access_token: accessToken,
                metric_type: "total_value",
                cancellationToken: cancellationToken
            );

            if (response.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<OverviewStatistics>(OverviewStatistics.InvalidData);
            }

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<OverviewStatistics> result = OverviewStatistics.Create(response, userActivityLevel);

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            OverviewStatistics overviewStatistics = result.Value;
            overviewStatistics.InstagramAccountId = instagramAccount.Id.Value;
            overviewStatistics.StatisticsPeriod = statisticsPeriod;

            await this._overviewRepository.ReplaceAsync(overviewStatistics, statisticsPeriod, cancellationToken);

            return overviewStatistics;
        }
        catch
        {
            return Result.Failure<OverviewStatistics>(OverviewStatistics.InvalidData);
        }
    }

    public async Task<Result<InteractionStatistics>> GetInteractionsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        InteractionStatistics? validData = await this._interactionRepository.GetValidAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (validData is not null)
        {
            return validData;
        }

        InteractionStatistics? fallbackData = await this._interactionRepository.GetAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<InteractionStatistics>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        var dateRange = new InstagramPeriodCalculator(statisticsPeriod);

        try
        {
            JsonElement insights = await this._instagramStatisticsApi.GetInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "total_interactions,reach,likes,comments",
                period: "day",
                since: dateRange.Since.ToString(CultureInfo.InvariantCulture),
                until: dateRange.Until.ToString(CultureInfo.InvariantCulture),
                access_token: accessToken,
                metric_type: "total_value",
                cancellationToken: cancellationToken
            );

            if (insights.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<InteractionStatistics>(InteractionStatistics.InvalidData);
            }

            JsonElement ads = await this._instagramStatisticsApi.GetAdInsightsAsync(
                instagramAccount.Metadata.Id,
                fields: "spend",
                since: dateRange.Since.ToString(CultureInfo.InvariantCulture),
                until: dateRange.Until.ToString(CultureInfo.InvariantCulture),
                access_token: accessToken,
                cancellationToken: cancellationToken
            );

            if (ads.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<InteractionStatistics>(InteractionStatistics.InvalidData);
            }

            int daysCount = dateRange.Until.DayNumber - dateRange.Since.DayNumber + 1;

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<InteractionStatistics> result =
                InteractionStatistics.Create(
                    insights,
                    ads,
                    daysCount,
                    userActivityLevel
                );

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            InteractionStatistics interactionStatistics = result.Value;
            interactionStatistics.InstagramAccountId = instagramAccount.Id.Value;
            interactionStatistics.StatisticsPeriod = statisticsPeriod;

            await this._interactionRepository.ReplaceAsync(interactionStatistics, statisticsPeriod, cancellationToken);

            return interactionStatistics;
        }
        catch
        {
            return Result.Failure<InteractionStatistics>(InteractionStatistics.InvalidData);
        }
    }

    public async Task<Result<EngagementStatistics>> GetEngagementStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        EngagementStatistics? validData = await this._engagementRepository.GetValidAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (validData is not null)
        {
            return validData;
        }

        EngagementStatistics? fallbackData = await this._engagementRepository.GetAsync(
            instagramAccount.Id.Value,
            statisticsPeriod,
            cancellationToken
        );

        if (instagramAccount.Token is null)
        {
            return fallbackData ?? Result.Failure<EngagementStatistics>(TokenErrors.NotFound);
        }

        string accessToken = instagramAccount.Token.AccessToken.Value;

        var dateRange = new InstagramPeriodCalculator(statisticsPeriod);

        try
        {
            JsonElement response = await this._instagramStatisticsApi.GetInsightsAsync(
                instagramAccount.Metadata.Id,
                metric: "reach,impressions,profile_views,likes,comments,saves",
                period: "day",
                since: dateRange.Since.ToString(CultureInfo.InvariantCulture),
                until: dateRange.Until.ToString(CultureInfo.InvariantCulture),
                access_token: accessToken,
                metric_type: "total_value",
                cancellationToken: cancellationToken
            );

            if (response.ValueKind is JsonValueKind.Undefined)
            {
                return fallbackData ?? Result.Failure<EngagementStatistics>(EngagementStatistics.InvalidData);
            }

            UserActivityLevel userActivityLevel = await this._userActivityService.GetUserActivityLevelAsync(
                instagramAccount.UserId.Value,
                cancellationToken
            );

            Result<EngagementStatistics> result = EngagementStatistics.Create(
                response,
                instagramAccount.Metadata.FollowersCount,
                userActivityLevel
            );

            if (result.IsFailure)
            {
                return fallbackData ?? result;
            }

            EngagementStatistics engagementStatistics = result.Value;
            engagementStatistics.InstagramAccountId = instagramAccount.Id.Value;
            engagementStatistics.StatisticsPeriod = statisticsPeriod;

            await this._engagementRepository.ReplaceAsync(engagementStatistics, statisticsPeriod, cancellationToken);

            return engagementStatistics;
        }
        catch
        {
            return Result.Failure<EngagementStatistics>(EngagementStatistics.InvalidData);
        }
    }
}
