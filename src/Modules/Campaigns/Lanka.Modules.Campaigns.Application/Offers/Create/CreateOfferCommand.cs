using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Offers;

namespace Lanka.Modules.Campaigns.Application.Offers.Create;

public sealed record CreateOfferCommand(
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    string Description
) : ICommand<OfferId>;
