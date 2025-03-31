using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Offers.GetOffer;

namespace Lanka.Modules.Campaigns.Application.Offers.Edit;

public sealed record EditOfferCommand(
    Guid OfferId,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    string Description
) : ICommand<OfferResponse>;
