using Lanka.Modules.Campaigns.Domain.Offers;

namespace Lanka.Modules.Campaigns.Domain.Campaigns;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(CampaignId id, CancellationToken cancellationToken = default);
        
    Task<bool> IsAlreadyStartedAsync(
        Offer offer, 
        DateTimeOffset scheduledOnUtc,
        CancellationToken cancellationToken = default
    );
        
    void Add(Campaign campaign);
}
