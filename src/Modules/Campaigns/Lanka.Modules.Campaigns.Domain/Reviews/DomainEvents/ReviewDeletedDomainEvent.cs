using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Reviews.DomainEvents;

public sealed class ReviewDeletedDomainEvent(ReviewId reviewId) : DomainEvent
{
    public ReviewId ReviewId { get; init; } = reviewId;
}

