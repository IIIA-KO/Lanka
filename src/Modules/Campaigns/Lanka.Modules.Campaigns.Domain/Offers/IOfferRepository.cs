namespace Lanka.Modules.Campaigns.Domain.Offers;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(OfferId id, CancellationToken cancellationToken = default);
}