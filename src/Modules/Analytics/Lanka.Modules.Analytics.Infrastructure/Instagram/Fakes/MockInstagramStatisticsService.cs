using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramStatisticsService : IInstagramStatisticsService
{
    public Task<Result<MetricsStatistics>> GetMetricsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        MetricsStatistics metrics = FakeInstagramDataGenerator.GenerateMetricsStatistics(
            instagramAccount.Id.Value,
            statisticsPeriod
        );

        return Task.FromResult<Result<MetricsStatistics>>(metrics);
    }

    public Task<Result<OverviewStatistics>> GetOverviewStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        OverviewStatistics overview = FakeInstagramDataGenerator.GenerateOverviewStatistics(
            instagramAccount.Id.Value,
            statisticsPeriod
        );

        return Task.FromResult<Result<OverviewStatistics>>(overview);
    }

    public Task<Result<InteractionStatistics>> GetInteractionsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        InteractionStatistics interactions = FakeInstagramDataGenerator.GenerateInteractionStatistics(
            instagramAccount.Id.Value,
            statisticsPeriod
        );

        return Task.FromResult<Result<InteractionStatistics>>(interactions);
    }

    public Task<Result<EngagementStatistics>> GetEngagementStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        EngagementStatistics engagement = FakeInstagramDataGenerator.GenerateEngagementStatistics(
            instagramAccount.Id.Value,
            statisticsPeriod
        );

        return Task.FromResult<Result<EngagementStatistics>>(engagement);
    }
}
