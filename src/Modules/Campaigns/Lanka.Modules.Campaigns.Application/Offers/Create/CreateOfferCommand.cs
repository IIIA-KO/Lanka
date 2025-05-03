using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Offers.Create;

public sealed record CreateOfferCommand(
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    string Description
) : ICommand<Guid>;
