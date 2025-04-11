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

    public int Rating { get; init; }

    public string Comment { get; init; }

    public DateTime CreatedOnUtc { get; init; }
}
