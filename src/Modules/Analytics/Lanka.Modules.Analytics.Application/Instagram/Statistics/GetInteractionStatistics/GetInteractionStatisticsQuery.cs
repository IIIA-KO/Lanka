using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Domain;

namespace Lanka.Modules.Analytics.Application.Instagram.Statistics.GetInteractionStatistics;

public sealed record GetInteractionStatisticsQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : ICachedQuery<InteractionStatisticsResponse>
{
    public string CacheKey => $"interaction-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
