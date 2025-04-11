using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Reviews.Ratings;

public static class RatingErrors
{
    public static readonly Error Invalid = Error.Failure(
        "Rating.Invalid",
        "The rating is invalid"
    );
}
