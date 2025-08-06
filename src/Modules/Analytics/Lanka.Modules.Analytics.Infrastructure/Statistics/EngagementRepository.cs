using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Infrastructure.Database;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Statistics;

internal sealed class EngagementRepository
{
    private readonly IMongoCollection<EngagementStatistics> _collection;

    public EngagementRepository(IMongoClient mongoClient)
    {
        IMongoDatabase database = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = database.GetCollection<EngagementStatistics>(DocumentDbSettings.EnagementStatistics);
    }

    public async Task<EngagementStatistics?> GetAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<EngagementStatistics> filter = Builders<EngagementStatistics>
            .Filter.Where(es =>
                es.InstagramAccountId == instagramAccountId
                && es.StatisticsPeriod == statisticsPeriod
            );

        EngagementStatistics engagementStatistics =
            await this._collection.Find(filter).SingleOrDefaultAsync(cancellationToken);

        return engagementStatistics;
    }

    public async Task<EngagementStatistics?> GetValidAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        EngagementStatistics? engagementStatistics =
            await this.GetAsync(instagramAccountId, statisticsPeriod, cancellationToken);

        if (engagementStatistics is null || engagementStatistics.IsExpired)
        {
            return null;
        }

        return engagementStatistics;
    }

    public async Task InsertAsync(
        EngagementStatistics engagementStatistics,
        CancellationToken cancellationToken = default
    )
    {
        await this._collection.InsertOneAsync(engagementStatistics, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(
        EngagementStatistics engagementStatistics,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<EngagementStatistics> filter = Builders<EngagementStatistics>
            .Filter.Where(es =>
                es.InstagramAccountId == engagementStatistics.InstagramAccountId
                && es.StatisticsPeriod == statisticsPeriod
            );

        var options = new ReplaceOptions { IsUpsert = true };

        await this._collection.ReplaceOneAsync(filter, engagementStatistics, options, cancellationToken);
    }

    public async Task Remove(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<EngagementStatistics> filter = Builders<EngagementStatistics>
            .Filter.Eq(es => es.InstagramAccountId, instagramAccountId);

        await this._collection.DeleteOneAsync(filter, cancellationToken);
    }
}
