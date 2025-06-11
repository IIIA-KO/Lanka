using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceReachRatio;

public sealed record GetAudienceReachRatioQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : ICachedQuery<ReachRatio>
{
    public string CacheKey =>
        $"audience-reach-{this.UserId}-{(int)this.StatisticsPeriod}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
