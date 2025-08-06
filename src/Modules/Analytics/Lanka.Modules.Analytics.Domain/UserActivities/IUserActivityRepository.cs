namespace Lanka.Modules.Analytics.Domain.UserActivities;

public interface IUserActivityRepository
{
    Task<UserActivity?> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InsertAsync(UserActivity userActivity, CancellationToken cancellationToken = default);
    Task ReplaceAsync(UserActivity userActivity, CancellationToken cancellationToken = default);
    Task Remove(Guid userId, CancellationToken cancellationToken = default);
}
