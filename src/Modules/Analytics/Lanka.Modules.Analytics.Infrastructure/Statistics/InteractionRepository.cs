using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Statistics;

internal sealed class InteractionRepository
{
    private readonly IMongoCollection<InteractionStatistics> _collection;

    public InteractionRepository(IMongoClient mongoClient)
    {
        IMongoDatabase database = mongoClient.GetDatabase(DocumentDbSettings.Database);

        this._collection = database.GetCollection<InteractionStatistics>(DocumentDbSettings.InteractionStatistics);
    }

    public async Task<InteractionStatistics?> GetAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<InteractionStatistics> filter = Builders<InteractionStatistics>
            .Filter.Where(@is =>
                @is.InstagramAccountId == instagramAccountId
                && @is.StatisticsPeriod == statisticsPeriod
            );

        InteractionStatistics interactionStatistics =
            await this._collection.Find(filter).SingleOrDefaultAsync(cancellationToken);

        return interactionStatistics;
    }

    public async Task<InteractionStatistics?> GetValidAsync(
        Guid instagramAccountId,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        InteractionStatistics? interactionStatistics = await this.GetAsync(instagramAccountId, statisticsPeriod, cancellationToken);

        if (interactionStatistics is null || interactionStatistics.IsExpired)
        {
            return null;
        }
        
        return interactionStatistics;
    }

    public async Task InsertAsync(InteractionStatistics interactionStatistics,
        CancellationToken cancellationToken = default)
    {
        await this._collection.InsertOneAsync(interactionStatistics, cancellationToken: cancellationToken);
    }

    public async Task ReplaceAsync(
        InteractionStatistics interactionStatistics,
        StatisticsPeriod statisticsPeriod,
        CancellationToken cancellationToken = default
    )
    {
        FilterDefinition<InteractionStatistics> filter = Builders<InteractionStatistics>
            .Filter.Where(@is =>
                @is.InstagramAccountId == interactionStatistics.InstagramAccountId
                && @is.StatisticsPeriod == statisticsPeriod
            );

        var options = new ReplaceOptions { IsUpsert = true };
        
        await this._collection.ReplaceOneAsync(filter, interactionStatistics, options, cancellationToken);
    }
}
