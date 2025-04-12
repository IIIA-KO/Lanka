using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Campaigns;

internal sealed class CampaignRepository : ICampaignRepository
{
    private readonly CampaignsDbContext _dbContext;

    public CampaignRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Campaign?> GetByIdAsync(CampaignId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Campaigns.FirstOrDefaultAsync(
            campaign => campaign.Id == id,
            cancellationToken
        );
    }

    public async Task<bool> IsAlreadyStartedAsync(
        Offer offer,
        DateTimeOffset scheduledOnUtc,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset startTime = scheduledOnUtc - TimeSpan.FromMinutes(5);
        DateTimeOffset endTime = scheduledOnUtc + TimeSpan.FromMinutes(5);

        return await this._dbContext.Campaigns
            .AsNoTracking()
            .AnyAsync(
                campaign => campaign.OfferId == offer.Id
                            && campaign.ScheduledOnUtc >= startTime
                            && campaign.ScheduledOnUtc <= endTime
                            && Campaign.ActiveCampaignStatuses.Contains(campaign.Status),
                cancellationToken
            );
    }

    public void Add(Campaign campaign)
    {
        this._dbContext.Campaigns.Add(campaign);
    }
}
