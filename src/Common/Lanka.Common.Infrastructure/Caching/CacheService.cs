using Lanka.Common.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

namespace Lanka.Common.Infrastructure.Caching;

internal sealed class CacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CacheService(
        HybridCache hybridCache,
        IConnectionMultiplexer connectionMultiplexer
    )
    {
        this._hybridCache = hybridCache;
        this._connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await this._hybridCache.GetOrCreateAsync(
                key,
                factory: _ => ValueTask.FromResult(default(T)),
                cancellationToken: cancellationToken
            );
        }
        catch
        {
            return default;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        IDatabase db = this._connectionMultiplexer.GetDatabase();
        return await db.KeyExistsAsync(new RedisKey(key));
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        HybridCacheEntryOptions options = CacheOptions.CreateHybrid(expiration);

        await this._hybridCache.SetAsync(
            key,
            value,
            options,
            tags: null,
            cancellationToken
        );
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await this._hybridCache.RemoveAsync(key, cancellationToken);

        IDatabase db = this._connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(new RedisKey(key));
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        HybridCacheEntryOptions options = CacheOptions.CreateHybrid(expiration);

        return await this._hybridCache.GetOrCreateAsync(
            key,
            factory,
            options,
            tags: null,
            cancellationToken
        );
    }
}
