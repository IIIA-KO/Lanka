using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Audience;

internal sealed class LocationDistributionRepository
{
    private readonly IMongoCollection<LocationDistribution> _collection;

    public LocationDistributionRepository(IMongoClient mongoClient)
    {
        IMongoDatabase? database = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = database.GetCollection<LocationDistribution>(DocumentDbSettings.LocationDistribution);
    }

    public async Task<LocationDistribution?> GetAsync(
        Guid instagramAccountId,
        LocationType? locationType,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<LocationDistribution> filter = Builders<LocationDistribution>
            .Filter
            .Where(ld => ld.InstagramAccountId == instagramAccountId && ld.LocationType == locationType);

        LocationDistribution? locationDistribution = await this._collection
            .Find(filter)
            .SingleOrDefaultAsync(cancellationToken);

        return locationDistribution;
    }

    public async Task<LocationDistribution?> GetValidAsync(
        Guid instagramAccountId,
        LocationType locationType,
        CancellationToken cancellationToken = default
    )
    {
        LocationDistribution? locationDistribution =
            await this.GetAsync(instagramAccountId, locationType, cancellationToken);

        if (locationDistribution is null || locationDistribution.IsExpired)
        {
            return null;
        }

        return locationDistribution;
    }

    public async Task InsertAsync(
        LocationDistribution locationDistribution,
        CancellationToken cancellationToken = default
    )
    {
        await this._collection.InsertOneAsync(locationDistribution, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(
        LocationDistribution locationDistribution,
        LocationType locationType,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<LocationDistribution> filter = Builders<LocationDistribution>
            .Filter
            .Where(ld =>
                ld.InstagramAccountId == locationDistribution.InstagramAccountId
                && ld.LocationType == locationType
            );

        var options = new ReplaceOptions { IsUpsert = true };

        await this._collection.ReplaceOneAsync(filter, locationDistribution, options, cancellationToken);
    }
}
