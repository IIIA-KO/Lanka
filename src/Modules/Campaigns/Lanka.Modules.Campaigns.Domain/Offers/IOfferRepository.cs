namespace Lanka.Modules.Campaigns.Domain.Offers;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(OfferId id, CancellationToken cancellationToken = default);
    
    Task<bool> HasActiveCampaignsAsync(Offer offer, CancellationToken cancellationToken = default);
    
    void Add(Offer offer);
    
    void Remove(Offer offer);
}
