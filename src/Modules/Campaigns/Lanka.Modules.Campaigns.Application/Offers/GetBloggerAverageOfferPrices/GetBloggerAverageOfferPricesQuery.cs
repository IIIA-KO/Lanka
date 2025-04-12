using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Offers.GetBloggerAverageOfferPrices;

public sealed record GetBloggerAverageOfferPricesQuery(Guid BloggerId)
    : IQuery<IReadOnlyList<AveragePriceResponse>>;
