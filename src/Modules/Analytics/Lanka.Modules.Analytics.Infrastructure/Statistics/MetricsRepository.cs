using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Statistics;

internal sealed class MetricsRepository
{
    private readonly IMongoCollection<MetricsStatistics> _collection;

    public MetricsRepository(IMongoClient mongoClient)
    {
        IMongoDatabase database = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = database.GetCollection<MetricsStatistics>(DocumentDbSettings.MetricsStatistics);
    }
    
    public async Task<MetricsStatistics?> GetAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<MetricsStatistics> filter = Builders<MetricsStatistics>
            .Filter.Where(ms => 
                ms.InstagramAccountId == instagramAccountId
                && ms.StatisticsPeriod == statisticsPeriod
            );

        MetricsStatistics interactionStatistics =
            await this._collection.Find(filter).SingleOrDefaultAsync(cancellationToken);

        return interactionStatistics;
    }

    public async Task<MetricsStatistics?> GetValidAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        MetricsStatistics? metricsStatistics = await this.GetAsync(instagramAccountId, statisticsPeriod, cancellationToken);

        if (metricsStatistics is null || metricsStatistics.IsExpired)
        {
            return null;
        }

        return metricsStatistics;
    }

    public async Task InsertAsync(
        MetricsStatistics metricsStatistics,
        CancellationToken cancellationToken = default
    )
    {
        await this._collection.InsertOneAsync(metricsStatistics, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(
        MetricsStatistics metricsStatistics,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<MetricsStatistics> filter = Builders<MetricsStatistics>
            .Filter.Where(ms => 
                ms.InstagramAccountId == metricsStatistics.InstagramAccountId
                && ms.StatisticsPeriod == statisticsPeriod
            );
        
        var options = new ReplaceOptions { IsUpsert = true };
        
        await this._collection.ReplaceOneAsync(filter, metricsStatistics, options, cancellationToken);
    }
}
