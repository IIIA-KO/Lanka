using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetAudienceGenderRatio;

public sealed record GetAudienceGenderRatioQuery(Guid UserId) : ICachedQuery<GenderRatio>
{
    public string CacheKey => $"audience-gender-{this.UserId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
