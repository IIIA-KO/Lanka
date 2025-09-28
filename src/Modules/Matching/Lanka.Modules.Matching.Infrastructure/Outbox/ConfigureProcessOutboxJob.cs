using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Matching.Infrastructure.Outbox;

internal sealed class ConfigureProcessOutboxJob(IOptions<OutboxOptions> outboxOptions)
    : IConfigureOptions<QuartzOptions>
{
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(ProcessOutboxJob).FullName!;

        options
            .AddJob<ProcessOutboxJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithIntervalInSeconds(this._outboxOptions.IntervalInSeconds)
                            .RepeatForever()
                    )
            );
    }
}
