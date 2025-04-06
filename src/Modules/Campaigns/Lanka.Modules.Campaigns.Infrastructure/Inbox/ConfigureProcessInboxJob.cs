using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Campaigns.Infrastructure.Inbox;

internal sealed class ConfigureProcessInboxJob(IOptions<InboxOptions> outboxOptions)
    : IConfigureOptions<QuartzOptions>
{
    private readonly InboxOptions _inboxOptions = outboxOptions.Value;

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(ProcessInboxJob).FullName!;

        options
            .AddJob<ProcessInboxJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(this._inboxOptions.IntervalInSeconds).RepeatForever()));
    }
}
