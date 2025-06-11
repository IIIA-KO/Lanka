using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceAgeRatio;

public sealed record GetAudienceAgeRatioQuery(Guid UserId) : ICachedQuery<AgeRatio>
{
    public string CacheKey => $"audience-age-{this.UserId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
