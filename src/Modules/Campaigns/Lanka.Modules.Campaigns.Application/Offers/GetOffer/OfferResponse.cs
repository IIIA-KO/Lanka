using Lanka.Modules.Campaigns.Domain.Offers;

namespace Lanka.Modules.Campaigns.Application.Offers.GetOffer;

public sealed class OfferResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; }
    public string Description { get; init; }

    public static OfferResponse FromOffer(Offer offer)
    {
        return new OfferResponse
        {
            Id = offer.Id.Value,
            Name = offer.Name.Value,
            PriceAmount = offer.Price.Amount,
            PriceCurrency = offer.Price.Currency.ToString(),
            Description = offer.Description.Value
        };
    }
}
