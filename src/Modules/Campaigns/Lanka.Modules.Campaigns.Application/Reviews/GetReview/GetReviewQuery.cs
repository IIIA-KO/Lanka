using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Reviews.GetReview;

public sealed record GetReviewQuery(Guid ReviewId) : IQuery<ReviewResponse>;
