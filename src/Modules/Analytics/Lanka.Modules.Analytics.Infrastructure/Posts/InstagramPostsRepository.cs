using Lanka.Modules.Analytics.Domain.Posts;
using Lanka.Modules.Analytics.Infrastructure.Database;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Posts;

internal sealed class InstagramPostsRepository
{
    private readonly IMongoCollection<InstagramPosts> _collection;

    public InstagramPostsRepository(IMongoClient mongoClient)
    {
        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = mongoDatabase.GetCollection<InstagramPosts>(DocumentDbSettings.InstagramPosts);
    }

    public async Task<InstagramPosts?> GetAsync(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<InstagramPosts> filter = Builders<InstagramPosts>
            .Filter.Eq(p => p.InstagramAccountId, instagramAccountId);

        InstagramPosts? posts = await this._collection
            .Find(filter)
            .SingleOrDefaultAsync(cancellationToken);

        return posts;
    }

    public async Task<InstagramPosts?> GetValidAsync(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        InstagramPosts? posts = await this.GetAsync(instagramAccountId, cancellationToken);

        if (posts is null || posts.IsExpired)
        {
            return null;
        }

        return posts;
    }

    public async Task ReplaceAsync(InstagramPosts posts, CancellationToken cancellationToken = default)
    {
        FilterDefinition<InstagramPosts> filter = Builders<InstagramPosts>
            .Filter.Eq(p => p.InstagramAccountId, posts.InstagramAccountId);

        var options = new ReplaceOptions { IsUpsert = true };

        await this._collection.ReplaceOneAsync(filter, posts, options, cancellationToken);
    }

    public async Task Remove(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<InstagramPosts> filter = Builders<InstagramPosts>
            .Filter.Eq(p => p.InstagramAccountId, instagramAccountId);

        await this._collection.DeleteOneAsync(filter, cancellationToken);
    }
}
