using System.Buffers;
using System.Text.Json;
using Lanka.Common.Application.Caching;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Lanka.Common.Infrastructure.Caching;

internal sealed class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
    {
        this._cache = cache;
        this._connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        byte[]? bytes = await this._cache.GetAsync(key, cancellationToken);

        return bytes is null ? default : Deserialize<T>(bytes);
    }
    
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        IDatabase db = this._connectionMultiplexer.GetDatabase();
        return  await db.KeyExistsAsync(new RedisKey(key));
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        byte[] bytes = Serialize(value);

        return this._cache.SetAsync(
            key,
            bytes,
            CacheOptions.Create(expiration),
            cancellationToken
        );
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        this._cache.RemoveAsync(key, cancellationToken);

    private static T Deserialize<T>(byte[] bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes)!;
    }

    private static byte[] Serialize<T>(T value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, value);
        return buffer.WrittenSpan.ToArray();
    }
}
