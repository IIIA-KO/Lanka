using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Reviews;

public static class ReviewErrors
{
    public static readonly Error NotEligible =
        Error.Problem(
            "Review.NotEligible",
            "The review is not eligible because the cooperation is not yet completed"
        );

    public static readonly Error AlreadyReviewed =
        Error.Conflict(
            "Review.AlreadyReviewed",
            "TThe user has already reviewed for this campaign"
        );

    public static readonly Error NotFound =
        Error.NotFound(
            "Review.NotFound",
            "Review with specified identifier was not found"
        );

    public static readonly Error NotOwner =
        Error.Unauthorized(
            "Review.NotOwner",
            "The user is not the owner of the review"
        );
}
