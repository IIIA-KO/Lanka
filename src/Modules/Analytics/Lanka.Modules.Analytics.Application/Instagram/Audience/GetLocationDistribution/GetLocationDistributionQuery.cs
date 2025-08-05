using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Domain.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;

public sealed record GetLocationDistributionQuery(Guid UserId, LocationType LocationType)
    : ICachedQuery<LocationDistributionResponse>
{
    public string CacheKey => $"audience-location-{this.UserId}-{(int)this.LocationType}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
