using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAgeDistribution;

public sealed record GetAgeDistributionQuery(Guid UserId) : ICachedQuery<AgeDistribution>
{
    public string CacheKey => $"audience-age-{this.UserId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
