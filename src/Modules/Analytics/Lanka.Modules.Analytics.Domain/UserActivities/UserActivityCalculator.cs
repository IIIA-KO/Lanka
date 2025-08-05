namespace Lanka.Modules.Analytics.Domain.UserActivities;

public static class UserActivityCalculator
{
    public static UserActivity Calculate(UserActivity userActivity, DateTimeOffset nowUtc)
    {
        DateTimeOffset thirtyDaysAgo = nowUtc.AddDays(-30);

        int loginPoints = userActivity.LastLoginAt > thirtyDaysAgo ? 1 : 0;

        int completedAsClient = userActivity.CampaignsCompletedAsClient
            .Count(ts => ts > thirtyDaysAgo);

        int completedAsCreator = userActivity.CampaignsCompletedCreator
            .Count(ts => ts > thirtyDaysAgo);

        int reviewPoints = userActivity.ReviewsWritten
            .Count(r => r.CreatedAt > thirtyDaysAgo);

        double score =
            loginPoints * 1.0
            + completedAsClient * 2.0
            + completedAsCreator * 3.0
            + reviewPoints * 2.0;

        UserActivityLevel level = score switch
        {
            <= 5 => UserActivityLevel.Inactive,
            <= 15 => UserActivityLevel.Occasional,
            <= 30 => UserActivityLevel.Active,
            _ => UserActivityLevel.PowerUser
        };

        userActivity.ActivityScore = score;
        userActivity.ActivityLevel = level.ToString();
        return userActivity;
    }
}
