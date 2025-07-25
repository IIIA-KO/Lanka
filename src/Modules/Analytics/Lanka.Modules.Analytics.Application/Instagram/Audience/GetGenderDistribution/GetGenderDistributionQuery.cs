using Lanka.Common.Application.Caching;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Audience;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetGenderDistribution;

public sealed record GetGenderDistributionQuery(Guid UserId) : ICachedQuery<GenderDistribution>
{
    public string CacheKey => $"audience-gender-{this.UserId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
