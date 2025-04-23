using Lanka.Modules.Campaigns.Application.Offers.GetOffer;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;

public sealed class PactResponse
{
    public Guid Id { get; init; }
    public Guid BloggerId { get; init; }
    public string Content { get; init; }
    public List<OfferResponse> Offers { get; init; }

    public static PactResponse FromPact(Pact pact)
    {
        return new PactResponse
        {
            Id = pact.Id.Value,
            BloggerId = pact.BloggerId.Value,
            Content = pact.Content.Value,
            Offers = pact.Offers.Select(OfferResponse.FromOffer).ToList()
        };
    }
}
