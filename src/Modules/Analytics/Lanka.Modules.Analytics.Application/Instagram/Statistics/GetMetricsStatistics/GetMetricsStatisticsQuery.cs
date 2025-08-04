using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Domain;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetMetricsStatistics;

public sealed record GetMetricsStatisticsQuery(Guid UserId, StatisticsPeriod StatisticsPeriod)
    : ICachedQuery<MetricsStatisticsResponse>
{
    public string CacheKey => $"table-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
