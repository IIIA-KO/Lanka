using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Reviews.EditReview;

public sealed record EditReviewCommand(
    Guid ReviewId,
    int Rating,
    string Comment
) : ICommand;
