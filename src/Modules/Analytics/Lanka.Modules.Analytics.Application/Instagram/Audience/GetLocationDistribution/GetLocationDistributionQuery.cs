using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetLocationDistribution;

public sealed record GetLocationDistributionQuery(Guid UserId, LocationType LocationType)
    : ICachedQuery<LocationDistribution>
{
    public string CacheKey => $"audience-location-{this.UserId}-{(int)this.LocationType}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
