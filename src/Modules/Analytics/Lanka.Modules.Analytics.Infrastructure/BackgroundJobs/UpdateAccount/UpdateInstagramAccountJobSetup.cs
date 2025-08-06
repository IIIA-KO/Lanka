using Lanka.Modules.Analytics.Infrastructure.Instagram;
using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.UpdateAccount;

internal sealed class UpdateInstagramAccountJobSetup : IConfigureOptions<QuartzOptions>
{
    private readonly InstagramOptions _instagramOptions;

    public UpdateInstagramAccountJobSetup(IOptions<InstagramOptions> instagramOptions)
    {
        this._instagramOptions = instagramOptions.Value;
    }

    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(UpdateInstagramAccountsJob);

        options
            .AddJob<UpdateInstagramAccountsJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithIntervalInSeconds(this._instagramOptions.IntervalInSeconds)
                            .RepeatForever()
                    )
            );
    }
}
