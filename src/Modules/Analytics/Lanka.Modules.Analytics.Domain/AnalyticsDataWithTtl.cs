using Lanka.Modules.Analytics.Domain.UserActivities;

namespace Lanka.Modules.Analytics.Domain;

public abstract class AnalyticsDataWithTtl
{
    public DateTimeOffset LastRefreshedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public TimeSpan TimeToLive { get; init; }

    public DateTimeOffset ExpiresAtUtc => this.LastRefreshedAtUtc.Add(this.TimeToLive);

    public bool IsExpired => DateTimeOffset.UtcNow >= this.ExpiresAtUtc;

    public bool IsValid => !this.IsExpired;

    protected AnalyticsDataWithTtl()
    {
        this.TimeToLive = TimeSpan.FromDays(7);
    }
    
    protected AnalyticsDataWithTtl(TimeSpan timeToLive)
    {
        this.TimeToLive = timeToLive;
    }
    
    protected static TimeSpan GetTtlForActivityLevel(UserActivityLevel activityLevel) => activityLevel switch
    {
        UserActivityLevel.PowerUser => TimeSpan.FromDays(3),
        UserActivityLevel.Active => TimeSpan.FromDays(7),
        UserActivityLevel.Occasional => TimeSpan.FromDays(14),
        UserActivityLevel.Inactive => TimeSpan.FromDays(21),
        _ => TimeSpan.FromDays(14)
    };

    public virtual void UpdateTtl(UserActivityLevel activityLevel)
    {
        
    }
}
