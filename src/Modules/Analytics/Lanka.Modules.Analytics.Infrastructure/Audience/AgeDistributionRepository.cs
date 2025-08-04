using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Audience;

internal sealed class AgeDistributionRepository
{
    private readonly IMongoCollection<AgeDistribution> _collection;

    public AgeDistributionRepository(IMongoClient mongoClient)
    {
        IMongoDatabase? mongoDatabase = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = mongoDatabase.GetCollection<AgeDistribution>(DocumentDbSettings.AgeDistribution);
    }

    public async Task<AgeDistribution?> GetAsync(
        Guid instagramAccoutId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<AgeDistribution> filter = Builders<AgeDistribution>
            .Filter.Eq(ad => ad.InstagramAccountId, instagramAccoutId);

        AgeDistribution ageDistribution = await this._collection
            .Find(filter)
            .SingleOrDefaultAsync(cancellationToken);

        return ageDistribution;
    }

    public async Task<AgeDistribution?> GetValidAsync(
        Guid instagramAccoutId,
        CancellationToken cancellationToken = default
    )
    {
        AgeDistribution? ageDistribution = await this.GetAsync(instagramAccoutId, cancellationToken);

        if (ageDistribution is null || ageDistribution.IsExpired)
        {
            return null;
        }

        return ageDistribution;
    }

    public async Task InsertAsync(AgeDistribution ageDistribution, CancellationToken cancellationToken = default)
    {
        await this._collection.InsertOneAsync(ageDistribution, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(AgeDistribution ageDistribution, CancellationToken cancellationToken = default)
    {
        FilterDefinition<AgeDistribution> filter = Builders<AgeDistribution>
            .Filter.Eq(ad => ad.InstagramAccountId, ageDistribution.InstagramAccountId);

        var options = new ReplaceOptions { IsUpsert = true };

        await this._collection.ReplaceOneAsync(filter, ageDistribution, options, cancellationToken);
    }
}
