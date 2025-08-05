using Lanka.Common.Application.Caching;

namespace Lanka.Modules.Analytics.Application.Instagram.Audience.GetGenderDistribution;

public sealed record GetGenderDistributionQuery(Guid UserId) : ICachedQuery<GenderDistributionResponse>
{
    public string CacheKey => $"audience-gender-{this.UserId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
}
