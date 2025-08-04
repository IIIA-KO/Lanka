using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.CleanupExpiredAnalytics;

public class CleanupExpiredAnalyticsJobSetup : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(CleanupExpiredAnalyticsJob);

        options
            .AddJob<CleanupExpiredAnalyticsJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithCronSchedule("0 0 3 * * ?")
            );
    }
}
