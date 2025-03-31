using Lanka.Modules.Campaigns.Domain.Campaigns;
using Lanka.Modules.Campaigns.Domain.Offers;
using Lanka.Modules.Campaigns.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Campaigns.Infrastructure.Offers;

internal sealed class OfferRepository : IOfferRepository
{
    private readonly CampaignsDbContext _dbContext;

    public OfferRepository(CampaignsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<Offer?> GetByIdAsync(OfferId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Offers.FirstOrDefaultAsync(
            offer => offer.Id == id, 
            cancellationToken
        );
    }

    public async Task<bool> HasActiveCampaignsAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        return await this.
            _dbContext.Campaigns
            .AsNoTracking()
            .AnyAsync(campaign => campaign.OfferId == offer.Id 
                                  && Campaign.ActiveCampaignStatuses.Contains(campaign.Status), 
                cancellationToken
            );
    }
    
    public void Add(Offer offer)
    {
        this._dbContext.Offers.Add(offer);
    }

    public void Remove(Offer offer)
    {
        this._dbContext.Offers.Remove(offer);
    }
}
