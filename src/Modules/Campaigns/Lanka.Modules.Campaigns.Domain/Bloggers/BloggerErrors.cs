using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public static class BloggerErrors
{
    public static readonly Error NotFound =
        Error.NotFound(
            "Blogger.NotFound",
            "The blogger with the specified identifier was not found"
        );
    
    public static Error InvalidCategory => Error.Validation(
        "Blogger.InvalidCategory",
        "The provided account category is not supported."
    );

    public static readonly Error ActiveCampaignsExist =
        Error.Conflict(
            "Blogger.ActiveCampaignsExist",
            "Cannot delete profile or change payout currency while active campaigns exist."
        );

    public static readonly Error PayoutAccountRequired =
        Error.Validation(
            "Blogger.PayoutAccountRequired",
            "A payout account must be configured before creating offers."
        );

    public static readonly Error OfferCurrencyMismatch =
        Error.Validation(
            "Blogger.OfferCurrencyMismatch",
            "Offer currency must match your payout account currency."
        );
}
