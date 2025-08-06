using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Infrastructure.Database;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Statistics;

internal sealed class OverviewRepository
{
    private readonly IMongoCollection<OverviewStatistics> _collection;

    public OverviewRepository(IMongoClient mongoClient)
    {
        IMongoDatabase database = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = database.GetCollection<OverviewStatistics>(DocumentDbSettings.OverviewStatistics);
    }

    public async Task<OverviewStatistics?> GetAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<OverviewStatistics> filter = Builders<OverviewStatistics>
            .Filter.Where(os =>
                os.InstagramAccountId == instagramAccountId
                && os.StatisticsPeriod == statisticsPeriod
            );

        OverviewStatistics overviewStatistics =
            await this._collection.Find(filter).SingleOrDefaultAsync(cancellationToken);

        return overviewStatistics;
    }

    public async Task<OverviewStatistics?> GetValidAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        OverviewStatistics? overviewStatistics =
            await this.GetAsync(instagramAccountId, statisticsPeriod, cancellationToken);

        if (overviewStatistics is null || overviewStatistics.IsExpired)
        {
            return null;
        }

        return overviewStatistics;
    }

    public async Task InsertAsync(
        OverviewStatistics overviewStatistics,
        CancellationToken cancellationToken = default
    )
    {
        await this._collection.InsertOneAsync(overviewStatistics, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(
        OverviewStatistics overviewStatistics,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<OverviewStatistics> filter = Builders<OverviewStatistics>
            .Filter.Where(os =>
                os.InstagramAccountId == overviewStatistics.InstagramAccountId
                && os.StatisticsPeriod == statisticsPeriod
            );

        var options = new ReplaceOptions { IsUpsert = true };

        await this._collection.ReplaceOneAsync(filter, overviewStatistics, options, cancellationToken);
    }

    public async Task Remove(
        Guid instagramAccountId,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<OverviewStatistics> filter = Builders<OverviewStatistics>
            .Filter.Eq(os => os.InstagramAccountId, instagramAccountId);

        await this._collection.DeleteOneAsync(filter, cancellationToken);
    }
}
