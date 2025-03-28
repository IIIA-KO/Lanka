namespace Lanka.Modules.Campaigns.Application.Offers.GetOffer;

public sealed record OfferResponse(
    Guid Id,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    string Description
);
