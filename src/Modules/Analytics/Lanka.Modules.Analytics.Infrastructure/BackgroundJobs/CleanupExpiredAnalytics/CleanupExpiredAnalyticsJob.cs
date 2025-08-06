using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.CleanupExpiredAnalytics;

[DisallowConcurrentExecution]
public class CleanupExpiredAnalyticsJob : IJob
{
    private readonly IMongoCleanupService _mongoCleanupService;
    private readonly ILogger<CleanupExpiredAnalyticsJob> _logger;

    public CleanupExpiredAnalyticsJob(IMongoCleanupService mongoCleanupService, ILogger<CleanupExpiredAnalyticsJob> logger)
    {
        this._mongoCleanupService = mongoCleanupService;
        this._logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        this._logger.LogInformation("Starting expired analytics cleanup job");

        DateTimeOffset cutoffTime = DateTimeOffset.UtcNow.AddDays(-30);
        CancellationToken cancellationToken = context.CancellationToken;

        try
        {
            await this._mongoCleanupService.DeleteExpiredDocumentsAsync(cutoffTime, cancellationToken);
            this._logger.LogInformation("All cleanup tasks completed successfully.");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to execute analytics cleanup job");
        }
    }
}
