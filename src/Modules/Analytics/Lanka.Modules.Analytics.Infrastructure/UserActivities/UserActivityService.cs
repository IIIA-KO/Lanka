using Lanka.Modules.Analytics.Application.Instagram.UserActivity;
using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Infrastructure.UserActivities;

internal sealed class UserActivityService : IUserActivityService
{
    private readonly IUserActivityRepository _userActivityRepository;

    public UserActivityService(IUserActivityRepository userActivityRepository)
    {
        this._userActivityRepository = userActivityRepository;
    }

    public async Task<UserActivityLevel> GetUserActivityLevelAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        UserActivity? userActivity = await this._userActivityRepository.GetAsync(userId, cancellationToken);

        if (userActivity is null)
        {
            return UserActivityLevel.Inactive;
        }

        UserActivity calculatedActivity = UserActivityCalculator.Calculate(userActivity, DateTimeOffset.UtcNow);
        
        return Enum.TryParse(calculatedActivity.ActivityLevel, out UserActivityLevel level) 
            ? level 
            : UserActivityLevel.Inactive;
    }
}
