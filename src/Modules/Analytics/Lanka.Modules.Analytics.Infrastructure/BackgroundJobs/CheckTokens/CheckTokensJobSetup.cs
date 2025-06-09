using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Analytics.Infrastructure.BackgroundJobs.CheckTokens;

internal sealed class CheckTokensJobSetup : IConfigureOptions<QuartzOptions>
{
    private readonly TokenOptions _tokenOptions;

    public CheckTokensJobSetup(IOptions<TokenOptions> tokenOptions)
    {
        this._tokenOptions = tokenOptions.Value;
    }
    
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(CheckTokensJob);
        
        options
            .AddJob<CheckTokensJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithIntervalInHours(this._tokenOptions.IntervalInHours)
                            .RepeatForever()
                    )
            );
    }
}
