using Lanka.Modules.Campaigns.Application.Offers.GetOffer;

namespace Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;

public sealed record PactResponse(
    Guid Id,
    Guid BloggerId,
    string Content,
    List<OfferResponse> Offers
);
