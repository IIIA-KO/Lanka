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
}
