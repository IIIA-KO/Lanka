using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Domain;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetEngagementStatistics;

public sealed record GetEngagementStatisticsQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : ICachedQuery<EngagementStatisticsResponse>
{
    public string CacheKey => $"engagement-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
