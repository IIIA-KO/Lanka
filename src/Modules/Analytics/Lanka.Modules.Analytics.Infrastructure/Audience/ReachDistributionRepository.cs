using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Infrastructure.Database;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Audience;

internal sealed class ReachDistributionRepository
{
    private readonly IMongoCollection<ReachDistribution> _collection;

    public ReachDistributionRepository(IMongoClient mongoClient)
    {
        IMongoDatabase? database = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = database.GetCollection<ReachDistribution>(DocumentDbSettings.ReachDistribution);
    }

    public async Task<ReachDistribution?> GetAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<ReachDistribution> filter = Builders<ReachDistribution>
            .Filter
            .Where(ld => ld.InstagramAccountId == instagramAccountId && ld.StatisticsPeriod == statisticsPeriod);

        ReachDistribution? reachDistribution = await this._collection
            .Find(filter).SingleOrDefaultAsync(cancellationToken);

        return reachDistribution;
    }

    public async Task<ReachDistribution?> GetValidAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        ReachDistribution? reachDistribution =
            await this.GetAsync(instagramAccountId, statisticsPeriod, cancellationToken);

        if (reachDistribution is null || reachDistribution.IsExpired)
        {
            return null;
        }

        return reachDistribution;
    }

    public async Task InsertAsync(
        ReachDistribution reachDistribution,
        CancellationToken cancellationToken = default
    )
    {
        await this._collection.InsertOneAsync(reachDistribution, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(
        ReachDistribution reachDistribution,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<ReachDistribution> filter = Builders<ReachDistribution>
            .Filter
            .Where(ld =>
                ld.InstagramAccountId == reachDistribution.InstagramAccountId
                && ld.StatisticsPeriod == statisticsPeriod
            );

        var options = new ReplaceOptions { IsUpsert = true };

        await this._collection.ReplaceOneAsync(filter, reachDistribution, options, cancellationToken);
    }

    public async Task Remove(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<ReachDistribution> filter = Builders<ReachDistribution>
            .Filter.Eq(rd => rd.InstagramAccountId, instagramAccountId);

        await this._collection.DeleteOneAsync(filter, cancellationToken);
    }
}
