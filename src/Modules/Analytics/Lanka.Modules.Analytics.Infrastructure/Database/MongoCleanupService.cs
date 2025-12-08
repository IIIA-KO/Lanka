using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.Posts;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Domain.UserActivities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Lanka.Modules.Analytics.Infrastructure.Database;

internal sealed class MongoCleanupService : IMongoCleanupService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoCleanupService> _logger;

    public MongoCleanupService(IMongoClient mongoClient, ILogger<MongoCleanupService> logger)
    {
        this._database = mongoClient.GetDatabase(DocumentDbSettings.Database);
        this._logger = logger;
    }


    public async Task DeleteByInstagramAccountIdAsync(Guid instagramAccountId, CancellationToken cancellationToken = default)
    {
        Task[] cleanupTasks =
        [
            this.DeleteByInstagramAccountIdAsync<AgeDistribution>(DocumentDbSettings.AgeDistribution, instagramAccountId, cancellationToken),
            this.DeleteByInstagramAccountIdAsync<GenderDistribution>(DocumentDbSettings.GenderDistribution, instagramAccountId, cancellationToken),
            this.DeleteByInstagramAccountIdAsync<LocationDistribution>(DocumentDbSettings.LocationDistribution, instagramAccountId, cancellationToken),
            this.DeleteByInstagramAccountIdAsync<ReachDistribution>(DocumentDbSettings.ReachDistribution, instagramAccountId, cancellationToken),
            this.DeleteByInstagramAccountIdAsync<EngagementStatistics>(DocumentDbSettings.EnagementStatistics, instagramAccountId, cancellationToken),
            this.DeleteByInstagramAccountIdAsync<InteractionStatistics>(DocumentDbSettings.InteractionStatistics, instagramAccountId, cancellationToken),
            this.DeleteByInstagramAccountIdAsync<MetricsStatistics>(DocumentDbSettings.MetricsStatistics, instagramAccountId, cancellationToken),
            this.DeleteByInstagramAccountIdAsync<OverviewStatistics>(DocumentDbSettings.OverviewStatistics, instagramAccountId, cancellationToken)
        ];

        await Task.WhenAll(cleanupTasks);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IMongoCollection<UserActivity>? collection = this._database.GetCollection<UserActivity>(DocumentDbSettings.UserActivity);
        FilterDefinition<UserActivity>? filter = Builders<UserActivity>.Filter.Eq(x => x.UserId, userId);
        await collection.DeleteManyAsync(filter, cancellationToken);
    }

    public async Task DeleteExpiredDocumentsAsync(DateTimeOffset cutoffTime, CancellationToken cancellationToken = default)
    {
        Task[] cleanupTasks =
        [
            this.DeleteExpiredDocumentsAsync<AgeDistribution>(DocumentDbSettings.AgeDistribution, "age distributions", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<GenderDistribution>(DocumentDbSettings.GenderDistribution, "gender distributions", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<LocationDistribution>(DocumentDbSettings.LocationDistribution, "location distributions", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<ReachDistribution>(DocumentDbSettings.ReachDistribution, "reach distributions", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<EngagementStatistics>(DocumentDbSettings.EnagementStatistics, "engagement statistics", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<InteractionStatistics>(DocumentDbSettings.InteractionStatistics, "interaction statistics", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<MetricsStatistics>(DocumentDbSettings.MetricsStatistics, "metrics statistics", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<OverviewStatistics>(DocumentDbSettings.OverviewStatistics, "overview statistics", cutoffTime, cancellationToken),
            this.DeleteExpiredDocumentsAsync<InstagramPosts>(DocumentDbSettings.InstagramPosts, "instagram posts", cutoffTime, cancellationToken)
        ];

        await Task.WhenAll(cleanupTasks);
    }

    private async Task DeleteByInstagramAccountIdAsync<T>(
        string collectionName, Guid instagramAccountId,
        CancellationToken cancellationToken
    ) where T : class
    {
        IMongoCollection<T>? collection = this._database.GetCollection<T>(collectionName);
        FilterDefinition<T>? filter = Builders<T>.Filter.Eq("_id", instagramAccountId);
        await collection.DeleteManyAsync(filter, cancellationToken);
    }

    private async Task DeleteExpiredDocumentsAsync<T>(
        string collectionName,
        string logName,
        DateTimeOffset cutoff,
        CancellationToken cancellationToken
    ) where T : AnalyticsDataWithTtl
    {
        IMongoCollection<T>? collection = this._database.GetCollection<T>(collectionName);
        FilterDefinition<T>? filter = Builders<T>.Filter.Lt(x => x.ExpiresAtUtc, cutoff);
        DeleteResult? result = await collection.DeleteManyAsync(filter, cancellationToken);

        this._logger.LogInformation("Cleanup completed. Deleted {Count} {Name}", result.DeletedCount, logName);
    }
}
