using Lanka.Common.Application.Clock;
using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Lanka.Modules.Campaigns.Infrastructure.Campaigns;

[DisallowConcurrentExecution]
internal sealed class ExpirePendingCampaignsJob : IJob
{
    private const string ModuleName = "Campaigns";

    private readonly CampaignsDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IOptions<ExpiredCampaignsOptions> _options;
    private readonly ILogger<ExpirePendingCampaignsJob> _logger;

    public ExpirePendingCampaignsJob(
        CampaignsDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        IOptions<ExpiredCampaignsOptions> options,
        ILogger<ExpirePendingCampaignsJob> logger
    )
    {
        this._dbContext = dbContext;
        this._dateTimeProvider = dateTimeProvider;
        this._options = options;
        this._logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        DateTimeOffset utcNow = this._dateTimeProvider.UtcNow;
        int batchSize = Math.Max(1, this._options.Value.BatchSize);

        List<Campaign> campaigns = await this._dbContext.Campaigns
            .Where(c => c.Status == CampaignStatus.Pending && c.ScheduledOnUtc <= utcNow)
            .OrderBy(c => c.ScheduledOnUtc)
            .Take(batchSize)
            .ToListAsync(context.CancellationToken);

        if (campaigns.Count == 0)
        {
            return;
        }

        foreach (Campaign campaign in campaigns)
        {
            campaign.Expire(utcNow);
        }

        await this._dbContext.SaveChangesAsync(context.CancellationToken);

        this._logger.LogInformation(
            "{Module} - Expired {CampaignCount} pending campaigns",
            ModuleName,
            campaigns.Count);
    }
}
