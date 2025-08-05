using Lanka.Modules.Analytics.Domain;
using Lanka.Modules.Analytics.Domain.Audience;
using Lanka.Modules.Analytics.Domain.Statistics;
using Lanka.Modules.Analytics.Infrastructure.Instagram;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.CleanupExpiredAnalytics;

[DisallowConcurrentExecution]
public class CleanupExpiredAnalyticsJob : IJob
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<CleanupExpiredAnalyticsJob> _logger;

    public CleanupExpiredAnalyticsJob(IMongoClient mongoClient, ILogger<CleanupExpiredAnalyticsJob> logger)
    {
        this._database = mongoClient.GetDatabase(DocumentDbSettings.Database);
        this._logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        this._logger.LogInformation("Starting expired analytics cleanup job");

        DateTimeOffset cutoffTime = DateTimeOffset.UtcNow.AddDays(-30);
        CancellationToken cancellationToken = context.CancellationToken;

        try
        {
            Task[] cleanupTasks =
            [
                this.DeleteExpiredDocumentsAsync<AgeDistribution>(
                    DocumentDbSettings.AgeDistribution,
                    "age distributions", cutoffTime, cancellationToken
                ),
                this.DeleteExpiredDocumentsAsync<GenderDistribution>(
                    DocumentDbSettings.GenderDistribution,
                    "gender distributions", cutoffTime, cancellationToken
                ),
                this.DeleteExpiredDocumentsAsync<LocationDistribution>(
                    DocumentDbSettings.LocationDistribution,
                    "location distributions", cutoffTime, cancellationToken
                ),
                this.DeleteExpiredDocumentsAsync<ReachDistribution>(
                    DocumentDbSettings.ReachDistribution,
                    "reach distributions", cutoffTime, cancellationToken
                ),
                this.DeleteExpiredDocumentsAsync<EngagementStatistics>(
                    DocumentDbSettings.EnagementStatistics,
                    "engagement statistics", cutoffTime, cancellationToken
                ),
                this.DeleteExpiredDocumentsAsync<InteractionStatistics>(
                    DocumentDbSettings.InteractionStatistics,
                    "interaction statistics", cutoffTime, cancellationToken
                ),
                this.DeleteExpiredDocumentsAsync<MetricsStatistics>(
                    DocumentDbSettings.MetricsStatistics,
                    "metrics statistics", cutoffTime, cancellationToken
                ),
                this.DeleteExpiredDocumentsAsync<OverviewStatistics>(
                    DocumentDbSettings.OverviewStatistics,
                    "overview statistics", cutoffTime, cancellationToken
                )
            ];

            await Task.WhenAll(cleanupTasks);

            this._logger.LogInformation("All cleanup tasks completed successfully.");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to execute analytics cleanup job");
        }
    }

    private async Task DeleteExpiredDocumentsAsync<T>(
        string collectionName,
        string logName,
        DateTimeOffset cutoff,
        CancellationToken cancellationToken)
        where T : AnalyticsDataWithTtl
    {
        IMongoCollection<T>? collection = this._database.GetCollection<T>(collectionName);
        FilterDefinition<T>? filter = Builders<T>.Filter.Lt(x => x.ExpiresAtUtc, cutoff);
        DeleteResult? result = await collection.DeleteManyAsync(filter, cancellationToken);

        this._logger.LogInformation("Cleanup completed. Deleted {Count} {Name}", result.DeletedCount, logName);
    }
}
