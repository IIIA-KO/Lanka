using Lanka.Common.Application.Caching;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;

public sealed record GetAgeDistributionQuery(Guid UserId) : ICachedQuery<AgeDistributionResponse>
{
    public string CacheKey => $"audience-age-{this.UserId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
