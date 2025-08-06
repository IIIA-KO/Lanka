using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Infrastructure.Database;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Audience;

internal sealed class GenderDistributionRepository
{
    private readonly IMongoCollection<GenderDistribution> _collection;

    public GenderDistributionRepository(IMongoClient mongoClient)
    {
        IMongoDatabase? mongoDatabase = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = mongoDatabase.GetCollection<GenderDistribution>(DocumentDbSettings.GenderDistribution);
    }

    public async Task<GenderDistribution?> GetAsync(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<GenderDistribution> filter = Builders<GenderDistribution>
            .Filter.Eq(gd => gd.InstagramAccountId, instagramAccountId);

        GenderDistribution? genderDistribution = await this._collection
            .Find(filter)
            .SingleOrDefaultAsync(cancellationToken);

        return genderDistribution;
    }

    public async Task<GenderDistribution?> GetValidAsync(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        GenderDistribution? genderDistribution = await this.GetAsync(instagramAccountId, cancellationToken);

        if (genderDistribution is null || genderDistribution.IsExpired)
        {
            return null;
        }

        return genderDistribution;
    }
    
    public async Task InsertAsync(GenderDistribution genderDistribution, CancellationToken cancellationToken = default)
    {
        await this._collection.InsertOneAsync(genderDistribution, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(GenderDistribution genderDistribution, CancellationToken cancellationToken = default)
    {
        FilterDefinition<GenderDistribution> filter = Builders<GenderDistribution>
            .Filter.Eq(gd => gd.InstagramAccountId, genderDistribution.InstagramAccountId);

        var options = new ReplaceOptions { IsUpsert = true };

        await this._collection.ReplaceOneAsync(filter, genderDistribution, options, cancellationToken);
    }
    
    public async Task Remove(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<GenderDistribution> filter = Builders<GenderDistribution>
            .Filter.Eq(gd => gd.InstagramAccountId, instagramAccountId);
        
        await this._collection.DeleteOneAsync(filter, cancellationToken);
    }
}
