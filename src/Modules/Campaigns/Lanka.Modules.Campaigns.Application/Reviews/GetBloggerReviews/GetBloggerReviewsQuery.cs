using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Reviews.GetReview;

namespace Lanka.Modules.Campaigns.Application.Reviews.GetBloggerReviews;

public sealed record GetBloggerReviewsQuery(Guid BloggerId) : IQuery<IReadOnlyList<ReviewResponse>>;
