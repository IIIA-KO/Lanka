using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceLocationRatio;

public sealed record GetAudienceLocationRatioQuery(Guid UserId, LocationType LocationType)
    : ICachedQuery<LocationRatio>
{
    public string CacheKey => $"audience-location-{this.UserId}-{(int)this.LocationType}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
