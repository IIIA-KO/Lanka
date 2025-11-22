using Microsoft.Extensions.Caching.Hybrid;

namespace Lanka.Common.Infrastructure.Caching;

public static class CacheOptions
{
    public static HybridCacheEntryOptions CreateHybrid(TimeSpan? expiration = null)
    {
        TimeSpan effectiveExpiration = expiration ?? TimeSpan.FromMinutes(2);

        return new HybridCacheEntryOptions
        {
            Expiration = effectiveExpiration,
            LocalCacheExpiration = effectiveExpiration
        };
    }
}
