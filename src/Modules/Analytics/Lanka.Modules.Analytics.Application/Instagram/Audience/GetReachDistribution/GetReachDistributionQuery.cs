using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetReachDistribution;

public sealed record GetReachDistributionQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : ICachedQuery<ReachDistribution>
{
    public string CacheKey =>
        $"audience-reach-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
