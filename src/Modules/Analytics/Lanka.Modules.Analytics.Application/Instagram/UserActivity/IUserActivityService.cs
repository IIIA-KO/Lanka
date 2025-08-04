using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Application.Instagram.UserActivity;

public interface IUserActivityService
{
    Task<UserActivityLevel> GetUserActivityLevelAsync(Guid userId, CancellationToken cancellationToken = default);
}
