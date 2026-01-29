using Bogus;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Infrastructure.Instagram.Fakes;

internal sealed class MockInstagramStatisticsService : IInstagramStatisticsService
{
    private readonly Faker _faker = new();

    public Task<Result<MetricsStatistics>> GetMetricsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default)
    {
        var metrics = new MetricsStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            StatisticsPeriod = statisticsPeriod,
            Metrics =
            [
                new TimeSeriesMetricData
                {
                    Name = "impressions",
                    Values = Enumerable.Range(0, 30).ToDictionary(
                        i => DateTime.UtcNow.Date.AddDays(-i),
                        _ => this._faker.Random.Int(100, 5000))
                },
                new TimeSeriesMetricData
                {
                    Name = "reach",
                    Values = Enumerable.Range(0, 30).ToDictionary(
                        i => DateTime.UtcNow.Date.AddDays(-i),
                        _ => this._faker.Random.Int(50, 4000))
                }
            ]
        };

        return Task.FromResult<Result<MetricsStatistics>>(metrics);
    }

    public Task<Result<OverviewStatistics>> GetOverviewStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default)
    {
        var overview = new OverviewStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            StatisticsPeriod = statisticsPeriod,
            Metrics =
            [
                new TotalValueMetricData { Name = "followers_count", Value = this._faker.Random.Int(1000, 50000) },
                new TotalValueMetricData { Name = "reach", Value = this._faker.Random.Int(5000, 100000) },
                new TotalValueMetricData { Name = "impressions", Value = this._faker.Random.Int(10000, 200000) }
            ]
        };

        return Task.FromResult<Result<OverviewStatistics>>(overview);
    }

    public Task<Result<InteractionStatistics>> GetInteractionsStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default)
    {
        var interactions = new InteractionStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            StatisticsPeriod = statisticsPeriod,
            EngagementRate = this._faker.Random.Double(1, 10),
            AverageLikes = this._faker.Random.Double(50, 500),
            AverageComments = this._faker.Random.Double(5, 50),
            CPE = this._faker.Random.Double(0.1, 2.0)
        };

        return Task.FromResult<Result<InteractionStatistics>>(interactions);
    }

    public Task<Result<EngagementStatistics>> GetEngagementStatistics(
        InstagramAccount instagramAccount,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default)
    {
        var engagement = new EngagementStatistics(UserActivityLevel.Active)
        {
            InstagramAccountId = instagramAccount.Id.Value,
            StatisticsPeriod = statisticsPeriod,
            ReachRate = this._faker.Random.Double(10, 50),
            EngagementRate = this._faker.Random.Double(1, 5),
            ERReach = this._faker.Random.Double(2, 15)
        };

        return Task.FromResult<Result<EngagementStatistics>>(engagement);
    }
}
