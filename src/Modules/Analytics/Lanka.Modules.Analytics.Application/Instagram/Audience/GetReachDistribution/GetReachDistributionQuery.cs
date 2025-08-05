using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Domain;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;

public sealed record GetReachDistributionQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : ICachedQuery<ReachDistributionResponse>
{
    public string CacheKey =>
        $"audience-reach-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
