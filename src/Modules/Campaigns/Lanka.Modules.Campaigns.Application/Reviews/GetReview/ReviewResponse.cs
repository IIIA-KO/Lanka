namespace Lanka.Modules.Campaigns.Application.Reviews.GetReview;

public sealed class ReviewResponse
{
    public ReviewResponse() { }

    public ReviewResponse(Guid id, int rating, string comment, DateTime createdOnUtc)
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

    public DateTime CreatedOnUtc { get; init; }
}
