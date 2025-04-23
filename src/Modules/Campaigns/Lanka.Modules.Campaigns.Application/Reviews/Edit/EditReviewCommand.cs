using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;

namespace Lanka.Modules.Campaigns.Application.Reviews.Edit;

public sealed record EditReviewCommand(
    Guid ReviewId,
    int Rating,
    string Comment
) : ICommand<ReviewResponse>;
