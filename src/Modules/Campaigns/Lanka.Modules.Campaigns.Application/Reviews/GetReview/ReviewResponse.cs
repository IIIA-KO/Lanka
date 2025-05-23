using Lanka.Modules.Campaigns.Domain.Reviews;

namespace Lanka.Modules.Campaigns.Application.Reviews.GetReview;

public sealed class ReviewResponse
{
    public ReviewResponse() { }

    public ReviewResponse(Guid id, int rating, string comment, DateTimeOffset createdOnUtc)
    {
        this.Id = id;
        this.Rating = rating;
        this.Comment = comment;
        this.CreatedOnUtc = createdOnUtc;
    }

    public Guid Id { get; init; }
    
    public Guid ClientId { get; init; }
    
    public Guid CreatorId { get; init; }
    
    public Guid OfferId { get; init; }
    
    public Guid CampaignId { get; init; }

    public int Rating { get; init; }

    public string Comment { get; init; }

    public DateTimeOffset CreatedOnUtc { get; init; }
    
    public static ReviewResponse FromReview(Review review)
    {
        return new ReviewResponse(
            review.Id.Value,
            review.Rating.Value,
            review.Comment.Value,
            review.CreatedOnUtc
        );
    }
}
