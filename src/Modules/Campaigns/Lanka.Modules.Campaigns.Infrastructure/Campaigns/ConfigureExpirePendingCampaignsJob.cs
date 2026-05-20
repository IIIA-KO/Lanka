using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Campaigns.Infrastructure.Campaigns;

internal sealed class ConfigureExpirePendingCampaignsJob
    : IConfigureOptions<QuartzOptions>
{
    private readonly ExpiredCampaignsOptions _options;

    public ConfigureExpirePendingCampaignsJob(IOptions<ExpiredCampaignsOptions> options)
    {
        this._options = options.Value;
    }

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(ExpirePendingCampaignsJob).FullName!;
        int intervalInSeconds = Math.Max(1, this._options.IntervalInSeconds);

        options
            .AddJob<ExpirePendingCampaignsJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithIntervalInSeconds(intervalInSeconds)
                            .RepeatForever()
                    )
            );
    }
}
