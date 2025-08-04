using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Domain;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetOverviewStatistics;

public sealed record GetOverviewStatisticsQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : ICachedQuery<OverviewStatisticsResponse>
{
    public string CacheKey => $"overview-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
