using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Offers
{
    public static class OfferErrors
    {
        public static readonly Error NotFound =
            Error.NotFound(
                "Offer.NotFound",
                "Offer with specified identifier could not be found"
                );
    }
}
