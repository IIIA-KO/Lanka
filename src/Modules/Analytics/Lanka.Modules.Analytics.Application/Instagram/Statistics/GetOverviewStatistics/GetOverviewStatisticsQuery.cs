using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Statistics;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;

public sealed record GetOverviewStatisticsQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : ICachedQuery<OverviewStatistics>
{
    public string CacheKey => $"overview-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
