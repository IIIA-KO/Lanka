using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Offers;

public static class OfferErrors
{
    public static readonly Error NotFound =
        Error.NotFound(
            "Offer.NotFound",
            "Offer with specified identifier could not be found"
        );

    public static readonly Error Duplicate =
        Error.Conflict("Offer.Duplicate", "Offer with specified name already exists");

    public static readonly Error HasActiveCampaigns =
        Error.Conflict("Offer.HasActiveCampaigns", "Offer has active campaigns");
}
