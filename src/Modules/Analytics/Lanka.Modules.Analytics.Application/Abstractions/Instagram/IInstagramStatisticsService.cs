using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Application.Abstractions.Instagram;

public interface IInstagramStatisticsService
{
    Task<Result<MetricsStatistics>> GetMetricsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    );

    Task<Result<OverviewStatistics>> GetOverviewStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    );

    Task<Result<InteractionStatistics>> GetInteractionsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    );

    Task<Result<EngagementStatistics>> GetEngagementStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    );
}
