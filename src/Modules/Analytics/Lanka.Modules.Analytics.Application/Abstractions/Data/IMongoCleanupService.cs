namespace Lanka.Modules.Analytics.Application.Abstractions.Data;

public interface IMongoCleanupService
{
    Task DeleteByInstagramAccountIdAsync(Guid instagramAccountId, CancellationToken cancellationToken = default);

    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task DeleteExpiredDocumentsAsync(DateTimeOffset cutoffTime, CancellationToken cancellationToken = default);
}
