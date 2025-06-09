namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.CheckTokens;

internal sealed class TokenOptions
{
    public int IntervalInHours { get; init; }
    
    public int RenewalThresholdInDays { get; init; }
}
